using WarHub.ArmouryModel.Source;

namespace WarHub.ArmouryModel.Concrete;

public class WhamCompilation : Compilation
{
    private SourceGlobalNamespaceSymbol? lazyGlobalNamespace;
    private Binder? lazyGlobalNamespaceBinder;
    private DiagnosticBag? lazyDeclarationDiagnostics;
    private DiagnosticBag? lazyConstraintDiagnostics;
    private ICategoryEntrySymbol? lazyNoCategoryEntrySymbol;
    private SymbolIndex? lazySymbolIndex;

    internal WhamCompilation(
        string? name,
        ImmutableArray<SourceTree> sourceTrees,
        CompilationOptions options,
        ImmutableArray<WhamCompilation> references = default)
        : base(name, sourceTrees, options)
    {
        References = references.IsDefault ? [] : references;
        ValidateInvariants();
    }

    /// <summary>
    /// Referenced compilations whose symbols are visible from this compilation.
    /// <para>
    /// A <b>catalogue compilation</b> has no references and contains only catalogue/gamesystem trees.
    /// A <b>roster compilation</b> references exactly one catalogue compilation and contains only roster trees.
    /// </para>
    /// </summary>
    public ImmutableArray<WhamCompilation> References { get; }

    /// <summary>
    /// <see langword="true"/> when this compilation references another compilation
    /// (i.e. this is a roster compilation).
    /// </summary>
    public bool HasReferences => References.Length > 0;

    public override IGamesystemNamespaceSymbol GlobalNamespace => SourceGlobalNamespace;

    internal SourceGlobalNamespaceSymbol SourceGlobalNamespace => GetGlobalNamespace();

    public static WhamCompilation Create()
    {
        return Create(ImmutableArray<SourceTree>.Empty);
    }

    public static WhamCompilation Create(
        ImmutableArray<SourceTree> sourceTrees,
        WhamCompilationOptions? options = null)
    {
        return new WhamCompilation(null, sourceTrees, options ?? new WhamCompilationOptions());
    }

    /// <summary>
    /// Creates a roster compilation that references a catalogue compilation.
    /// The <paramref name="catalogueCompilation"/> must not itself have references.
    /// </summary>
    public static WhamCompilation CreateRosterCompilation(
        ImmutableArray<SourceTree> rosterTrees,
        WhamCompilation catalogueCompilation,
        WhamCompilationOptions? options = null)
    {
        if (catalogueCompilation.HasReferences)
        {
            throw new ArgumentException(
                "The catalogue compilation must not itself have references (no chains).",
                nameof(catalogueCompilation));
        }
        return new WhamCompilation(
            null,
            rosterTrees,
            options ?? new WhamCompilationOptions(),
            [catalogueCompilation]);
    }

    public override SemanticModel GetSemanticModel(SourceTree tree)
    {
        throw new NotImplementedException();
    }

    public override ImmutableArray<Diagnostic> GetDiagnostics(CancellationToken cancellationToken = default)
    {
        ForceCompleteReferences(cancellationToken);
        SourceGlobalNamespace.ForceComplete(cancellationToken);
        var namespaceDiagnostics = SourceGlobalNamespace.DeclarationDiagnostics;
        var declarationDiagnostics = DeclarationDiagnostics;
        var constraintDiagnostics = ConstraintDiagnostics;
        var builder = ImmutableArray.CreateBuilder<Diagnostic>(
            namespaceDiagnostics.Count + declarationDiagnostics.Count + constraintDiagnostics.Count);
        builder.AddRange(namespaceDiagnostics.AsEnumerable());
        builder.AddRange(declarationDiagnostics.AsEnumerable());
        builder.AddRange(constraintDiagnostics.AsEnumerable());
        AggregateReferenceDiagnostics(builder, cancellationToken);
        return builder.DrainToImmutable();
    }

    public override ImmutableArray<Diagnostic> GetDeclarationDiagnostics(CancellationToken cancellationToken = default)
    {
        ForceCompleteReferences(cancellationToken);
        SourceGlobalNamespace.ForceComplete(cancellationToken);
        var namespaceDiagnostics = SourceGlobalNamespace.DeclarationDiagnostics;
        var declarationDiagnostics = DeclarationDiagnostics;
        var builder = ImmutableArray.CreateBuilder<Diagnostic>(
            namespaceDiagnostics.Count + declarationDiagnostics.Count);
        builder.AddRange(namespaceDiagnostics.AsEnumerable());
        builder.AddRange(declarationDiagnostics.AsEnumerable());
        foreach (var reference in References)
        {
            builder.AddRange(reference.GetDeclarationDiagnostics(cancellationToken));
        }
        return builder.DrainToImmutable();
    }

    public override ImmutableArray<Diagnostic> GetConstraintDiagnostics(CancellationToken cancellationToken = default)
    {
        ForceCompleteReferences(cancellationToken);
        SourceGlobalNamespace.ForceComplete(cancellationToken);
        var constraintDiagnostics = ConstraintDiagnostics;
        var builder = ImmutableArray.CreateBuilder<Diagnostic>(constraintDiagnostics.Count);
        builder.AddRange(constraintDiagnostics.AsEnumerable());
        foreach (var reference in References)
        {
            builder.AddRange(reference.GetConstraintDiagnostics(cancellationToken));
        }
        return builder.DrainToImmutable();
    }

    public override WhamCompilation AddSourceTrees(params SourceTree[] trees) =>
        Update(SourceTrees.AddRange(trees));

    public override WhamCompilation ReplaceSourceTree(SourceTree oldTree, SourceTree? newTree) =>
        Update(newTree is null ? SourceTrees.Remove(oldTree) : SourceTrees.Replace(oldTree, newTree));

    public override SourceTree? FindSourceTree(SourceNode rootNode)
    {
        var tree = base.FindSourceTree(rootNode);
        if (tree is not null)
            return tree;
        foreach (var reference in References)
        {
            tree = reference.FindSourceTree(rootNode);
            if (tree is not null)
                return tree;
        }
        return null;
    }

    private WhamCompilation Update(ImmutableArray<SourceTree> trees) =>
        new(Name, trees, Options, References);

    internal override ICatalogueSymbol CreateMissingGamesystemSymbol(DiagnosticBag diagnostics)
    {
        diagnostics.Add(ErrorCode.ERR_MissingGamesystem, Location.None);
        return new ErrorSymbols.ErrorGamesystemSymbol();
    }

    public override SymbolKeyResolution ResolveSymbolKey(SymbolKey key)
    {
        return GetSymbolIndex().Resolve(key);
    }

    private SymbolIndex GetSymbolIndex()
    {
        if (lazySymbolIndex is not null)
        {
            return lazySymbolIndex;
        }
        var index = SymbolIndex.Build(this);
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
        if (HasReferences)
        {
            // Roster compilation: take catalogue symbols from the referenced compilation.
            var referencedNamespace = References[0].SourceGlobalNamespace;
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
        if (References.Length == 0)
            return;

        // Roster compilations must reference exactly one catalogue compilation.
        if (References.Length != 1)
        {
            throw new InvalidOperationException(
                $"A roster compilation must reference exactly one catalogue compilation, " +
                $"but {References.Length} references were provided.");
        }

        // Roster compilations must not reference compilations that themselves have references.
        if (References[0].HasReferences)
        {
            throw new InvalidOperationException(
                "Referenced compilations must not themselves have references (no chains).");
        }

        // Roster compilations must contain only roster source trees.
        foreach (var tree in SourceTrees)
        {
            var root = tree.GetRoot();
            if (root is not Source.RosterNode)
            {
                throw new InvalidOperationException(
                    $"A roster compilation (with references) must contain only roster source trees, " +
                    $"but found a {root.GetType().Name}.");
            }
        }
    }

    private void ForceCompleteReferences(CancellationToken cancellationToken)
    {
        foreach (var reference in References)
        {
            reference.SourceGlobalNamespace.ForceComplete(cancellationToken);
        }
    }

    private void AggregateReferenceDiagnostics(
        ImmutableArray<Diagnostic>.Builder builder,
        CancellationToken cancellationToken)
    {
        foreach (var reference in References)
        {
            builder.AddRange(reference.GetDiagnostics(cancellationToken));
        }
    }
}
