using WarHub.ArmouryModel.Source;

namespace WarHub.ArmouryModel.Concrete;

public class WhamCompilation : Compilation
{
    private readonly WhamCompilation? _catalogueReference;
    private SourceGlobalNamespaceSymbol? lazyGlobalNamespace;
    private Binder? lazyGlobalNamespaceBinder;
    private DiagnosticBag? lazyDeclarationDiagnostics;
    private DiagnosticBag? lazyConstraintDiagnostics;
    private ICategoryEntrySymbol? lazyNoCategoryEntrySymbol;
    private SymbolIndex? lazySymbolIndex;

    /// <summary>
    /// Creates a catalogue compilation (no catalogue reference).
    /// </summary>
    private WhamCompilation(
        string? name,
        ImmutableArray<SourceTree> sourceTrees,
        CompilationOptions options)
        : base(name, sourceTrees, options)
    {
        _catalogueReference = null;
        ValidateInvariants();
    }

    /// <summary>
    /// Creates a roster compilation that inherits options from the catalogue reference.
    /// </summary>
    private WhamCompilation(
        string? name,
        ImmutableArray<SourceTree> rosterTrees,
        WhamCompilation catalogueReference)
        : base(name, rosterTrees, catalogueReference.Options)
    {
        _catalogueReference = catalogueReference;
        ValidateInvariants();
    }

    /// <summary>
    /// The referenced catalogue compilation whose symbols are visible from this compilation,
    /// or <see langword="null"/> for catalogue compilations.
    /// <para>
    /// A <b>catalogue compilation</b> has no catalogue reference and contains only catalogue/gamesystem trees.
    /// A <b>roster compilation</b> references exactly one catalogue compilation and contains only roster trees.
    /// Roster compilations always inherit <see cref="Compilation.Options"/> from their catalogue reference.
    /// </para>
    /// </summary>
    public override WhamCompilation? CatalogueReference => _catalogueReference;

    public override IGamesystemNamespaceSymbol GlobalNamespace => SourceGlobalNamespace;

    internal SourceGlobalNamespaceSymbol SourceGlobalNamespace => GetGlobalNamespace();

    /// <summary>
    /// Creates an empty catalogue compilation with default options.
    /// </summary>
    public static WhamCompilation Create()
    {
        return new WhamCompilation(null, ImmutableArray<SourceTree>.Empty, new WhamCompilationOptions());
    }

    /// <summary>
    /// Creates a compilation from the given source trees.
    /// If both catalogue/gamesystem and roster trees are present, automatically splits into
    /// a catalogue subcompilation referenced by a roster compilation, ensuring catalogue
    /// symbols are shared and not duplicated.
    /// </summary>
    /// <exception cref="ArgumentException">
    /// <paramref name="sourceTrees"/> contains only roster trees without any catalogue/gamesystem trees.
    /// </exception>
    public static WhamCompilation Create(
        ImmutableArray<SourceTree> sourceTrees,
        WhamCompilationOptions? options = null)
    {
        var opts = options ?? new WhamCompilationOptions();
        var catalogueTrees = ImmutableArray.CreateBuilder<SourceTree>();
        var rosterTrees = ImmutableArray.CreateBuilder<SourceTree>();
        foreach (var tree in sourceTrees)
        {
            if (tree.GetRoot() is RosterNode)
                rosterTrees.Add(tree);
            else
                catalogueTrees.Add(tree);
        }
        if (rosterTrees.Count > 0)
        {
            if (catalogueTrees.Count == 0)
            {
                throw new ArgumentException(
                    "Cannot create a compilation with only roster trees. " +
                    "Use CreateRosterCompilation to provide an explicit catalogue compilation reference.",
                    nameof(sourceTrees));
            }
            // Auto-split: catalogue subcompilation + roster compilation referencing it.
            var catComp = new WhamCompilation(null, catalogueTrees.ToImmutable(), opts);
            return new WhamCompilation(null, rosterTrees.ToImmutable(), catComp);
        }
        return new WhamCompilation(null, sourceTrees, opts);
    }

    /// <summary>
    /// Creates a roster compilation that references a catalogue compilation.
    /// The roster compilation inherits <see cref="Compilation.Options"/> from
    /// <paramref name="catalogueCompilation"/>.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// <paramref name="catalogueCompilation"/> is itself a roster compilation (no chains allowed).
    /// </exception>
    public static WhamCompilation CreateRosterCompilation(
        ImmutableArray<SourceTree> rosterTrees,
        WhamCompilation catalogueCompilation)
    {
        return new WhamCompilation(null, rosterTrees, catalogueCompilation);
    }

    public override SemanticModel GetSemanticModel(SourceTree tree)
    {
        throw new NotImplementedException();
    }

    public override ImmutableArray<Diagnostic> GetDiagnostics(CancellationToken cancellationToken = default)
    {
        ForceCompleteCatalogueReference(cancellationToken);
        SourceGlobalNamespace.ForceComplete(cancellationToken);
        var namespaceDiagnostics = SourceGlobalNamespace.DeclarationDiagnostics;
        var declarationDiagnostics = DeclarationDiagnostics;
        var constraintDiagnostics = ConstraintDiagnostics;
        var builder = ImmutableArray.CreateBuilder<Diagnostic>(
            namespaceDiagnostics.Count + declarationDiagnostics.Count + constraintDiagnostics.Count);
        builder.AddRange(namespaceDiagnostics.AsEnumerable());
        builder.AddRange(declarationDiagnostics.AsEnumerable());
        builder.AddRange(constraintDiagnostics.AsEnumerable());
        AggregateCatalogueReferenceDiagnostics(builder, cancellationToken);
        return builder.DrainToImmutable();
    }

    public override ImmutableArray<Diagnostic> GetDeclarationDiagnostics(CancellationToken cancellationToken = default)
    {
        ForceCompleteCatalogueReference(cancellationToken);
        SourceGlobalNamespace.ForceComplete(cancellationToken);
        var namespaceDiagnostics = SourceGlobalNamespace.DeclarationDiagnostics;
        var declarationDiagnostics = DeclarationDiagnostics;
        var builder = ImmutableArray.CreateBuilder<Diagnostic>(
            namespaceDiagnostics.Count + declarationDiagnostics.Count);
        builder.AddRange(namespaceDiagnostics.AsEnumerable());
        builder.AddRange(declarationDiagnostics.AsEnumerable());
        if (CatalogueReference is { } catRef)
        {
            builder.AddRange(catRef.GetDeclarationDiagnostics(cancellationToken));
        }
        return builder.DrainToImmutable();
    }

    public override ImmutableArray<Diagnostic> GetConstraintDiagnostics(CancellationToken cancellationToken = default)
    {
        ForceCompleteCatalogueReference(cancellationToken);
        SourceGlobalNamespace.ForceComplete(cancellationToken);
        var constraintDiagnostics = ConstraintDiagnostics;
        var builder = ImmutableArray.CreateBuilder<Diagnostic>(constraintDiagnostics.Count);
        builder.AddRange(constraintDiagnostics.AsEnumerable());
        if (CatalogueReference is { } catRef)
        {
            builder.AddRange(catRef.GetConstraintDiagnostics(cancellationToken));
        }
        return builder.DrainToImmutable();
    }

    /// <summary>
    /// Adds source trees to this compilation. Only same-kind trees are accepted:
    /// catalogue trees for catalogue compilations, roster trees for roster compilations.
    /// To add roster trees to a catalogue compilation, use <see cref="AddRosterTrees"/>.
    /// </summary>
    public override WhamCompilation AddSourceTrees(params SourceTree[] trees)
    {
        ValidateTreeTypesForAdd(trees);
        return Update(SourceTrees.AddRange(trees));
    }

    public override WhamCompilation AddRosterTrees(params SourceTree[] rosterTrees)
    {
        ValidateAllAreRosterTrees(rosterTrees, nameof(rosterTrees));
        if (CatalogueReference is not null)
        {
            // Already a roster compilation — add to own roster trees.
            return new WhamCompilation(Name, SourceTrees.AddRange(rosterTrees), CatalogueReference);
        }
        // Catalogue compilation — create roster compilation referencing this.
        return new WhamCompilation(Name, [.. rosterTrees], this);
    }

    /// <summary>
    /// Replaces a source tree in this compilation.
    /// The replacement tree must be the same kind (catalogue/roster) as the original.
    /// </summary>
    public override WhamCompilation ReplaceSourceTree(SourceTree oldTree, SourceTree? newTree)
    {
        if (newTree is not null)
        {
            var oldIsRoster = oldTree.GetRoot() is RosterNode;
            var newIsRoster = newTree.GetRoot() is RosterNode;
            if (oldIsRoster != newIsRoster)
            {
                throw new ArgumentException(
                    "Cannot replace a source tree with a tree of a different kind " +
                    "(e.g. replacing a catalogue tree with a roster tree).",
                    nameof(newTree));
            }
        }
        var trees = newTree is null ? SourceTrees.Remove(oldTree) : SourceTrees.Replace(oldTree, newTree);
        return Update(trees);
    }

    private WhamCompilation Update(ImmutableArray<SourceTree> trees)
    {
        if (CatalogueReference is { } catRef)
            return new WhamCompilation(Name, trees, catRef);
        return new WhamCompilation(Name, trees, Options);
    }

    internal override ICatalogueSymbol CreateMissingGamesystemSymbol(DiagnosticBag diagnostics)
    {
        diagnostics.Add(ErrorCode.ERR_MissingGamesystem, Location.None);
        return new ErrorSymbols.ErrorGamesystemSymbol();
    }

    public override SymbolKeyResolution ResolveSymbolKey(SymbolKey key)
    {
        return GetSymbolIndex().Resolve(key);
    }

    internal SymbolIndex GetSymbolIndex()
    {
        if (lazySymbolIndex is not null)
        {
            return lazySymbolIndex;
        }
        var catalogueIndex = CatalogueReference?.GetSymbolIndex();
        var index = SymbolIndex.Build(this, catalogueIndex);
        Interlocked.CompareExchange(ref lazySymbolIndex, index, null);
        return lazySymbolIndex;
    }

    internal Binder GetBinder(SourceNode node, ISymbol? containingSymbol)
    {
        var rootNode = node.AncestorsAndSelf().Last();
        return GetBinderFactory(rootNode.GetSourceTree(this)).GetBinder(node, containingSymbol);
    }

    internal BinderFactory GetBinderFactory(SourceTree tree)
    {
        return new BinderFactory(this, tree);
    }

    internal Binder GlobalNamespaceBinder => GetGlobalNamespaceBinder();

    private Binder GetGlobalNamespaceBinder()
    {
        if (lazyGlobalNamespaceBinder is not null)
        {
            return lazyGlobalNamespaceBinder;
        }
        var binder = new GamesystemNamespaceBinder(new BuckStopsHereBinder(this), SourceGlobalNamespace);
        Interlocked.CompareExchange(ref lazyGlobalNamespaceBinder, binder, null);
        return lazyGlobalNamespaceBinder;
    }

    private SourceGlobalNamespaceSymbol GetGlobalNamespace()
    {
        if (lazyGlobalNamespace is not null)
        {
            return lazyGlobalNamespace;
        }
        var newSymbol = CreateGlobalNamespace();
        Interlocked.CompareExchange(ref lazyGlobalNamespace, newSymbol, null);
        return lazyGlobalNamespace;
    }

    private SourceGlobalNamespaceSymbol CreateGlobalNamespace()
    {
        var nodes = SourceTrees.Select(x => x.GetRoot()).ToImmutableArray();
        if (CatalogueReference is { } catRef)
        {
            // Roster compilation: take catalogue symbols from the referenced compilation.
            var referencedNamespace = catRef.SourceGlobalNamespace;
            return new SourceGlobalNamespaceSymbol(nodes, this, referencedNamespace);
        }
        return new SourceGlobalNamespaceSymbol(nodes, this);
    }

    internal DiagnosticBag DeclarationDiagnostics
    {
        get
        {
            if (lazyDeclarationDiagnostics is null)
            {
                Interlocked.CompareExchange(ref lazyDeclarationDiagnostics, DiagnosticBag.GetInstance(), null);
            }
            return lazyDeclarationDiagnostics;
        }
    }

    internal DiagnosticBag ConstraintDiagnostics
    {
        get
        {
            if (lazyConstraintDiagnostics is null)
            {
                Interlocked.CompareExchange(ref lazyConstraintDiagnostics, DiagnosticBag.GetInstance(), null);
            }
            return lazyConstraintDiagnostics;
        }
    }

    public override ICategoryEntrySymbol NoCategoryEntrySymbol
    {
        get
        {
            if (lazyNoCategoryEntrySymbol is null)
            {
                Interlocked.CompareExchange(ref lazyNoCategoryEntrySymbol, CreateNoCategoryEntrySymbol(), null);
            }
            return lazyNoCategoryEntrySymbol;
        }
    }

    private CategoryEntrySymbol CreateNoCategoryEntrySymbol()
    {
        var node = NodeFactory.CategoryEntry("Uncategorised", "(No Category)");
        return new CategoryEntrySymbol(SourceGlobalNamespace, node, DeclarationDiagnostics);
    }

    private void ValidateInvariants()
    {
        if (CatalogueReference is null)
        {
            // Catalogue compilation: must not contain roster trees.
            foreach (var tree in SourceTrees)
            {
                if (tree.GetRoot() is RosterNode)
                {
                    throw new InvalidOperationException(
                        "A catalogue compilation must not contain roster source trees. " +
                        "Use Create() with mixed trees for auto-splitting, " +
                        "or CreateRosterCompilation() for explicit roster compilation.");
                }
            }
            return;
        }

        // Roster compilation: no chained references.
        if (CatalogueReference.HasCatalogueReference)
        {
            throw new InvalidOperationException(
                "Referenced compilations must not themselves have references (no chains).");
        }

        // Roster compilation: must contain only roster source trees.
        foreach (var tree in SourceTrees)
        {
            if (tree.GetRoot() is not RosterNode)
            {
                throw new InvalidOperationException(
                    $"A roster compilation must contain only roster source trees, " +
                    $"but found a {tree.GetRoot().GetType().Name}.");
            }
        }
    }

    private void ValidateTreeTypesForAdd(SourceTree[] trees)
    {
        if (HasCatalogueReference)
        {
            ValidateAllAreRosterTrees(trees, nameof(trees));
        }
        else
        {
            foreach (var tree in trees)
            {
                if (tree.GetRoot() is RosterNode)
                {
                    throw new ArgumentException(
                        "Cannot add roster trees to a catalogue compilation. " +
                        "Use AddRosterTrees() to create a roster compilation instead.",
                        nameof(trees));
                }
            }
        }
    }

    private static void ValidateAllAreRosterTrees(SourceTree[] trees, string paramName)
    {
        foreach (var tree in trees)
        {
            if (tree.GetRoot() is not RosterNode)
            {
                throw new ArgumentException(
                    "All trees must be roster trees.",
                    paramName);
            }
        }
    }

    private void ForceCompleteCatalogueReference(CancellationToken cancellationToken)
    {
        CatalogueReference?.SourceGlobalNamespace.ForceComplete(cancellationToken);
    }

    private void AggregateCatalogueReferenceDiagnostics(
        ImmutableArray<Diagnostic>.Builder builder,
        CancellationToken cancellationToken)
    {
        if (CatalogueReference is { } catRef)
        {
            builder.AddRange(catRef.GetDiagnostics(cancellationToken));
        }
    }
}
