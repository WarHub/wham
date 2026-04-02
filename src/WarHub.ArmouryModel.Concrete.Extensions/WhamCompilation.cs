using WarHub.ArmouryModel.Source;

namespace WarHub.ArmouryModel.Concrete;

public class WhamCompilation : Compilation
{
    private SourceGlobalNamespaceSymbol? lazyGlobalNamespace;
    private Binder? lazyGlobalNamespaceBinder;
    private DiagnosticBag? lazyDeclarationDiagnostics;
    private ICategoryEntrySymbol? lazyNoCategoryEntrySymbol;

    internal WhamCompilation(string? name, ImmutableArray<SourceTree> sourceTrees, CompilationOptions options)
        : base(name, sourceTrees, options)
    {
    }

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

    public override SemanticModel GetSemanticModel(SourceTree tree)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Returns binding and declaration diagnostics. Does not include constraint
    /// validation diagnostics — use <see cref="GetValidationDiagnostics"/> for those.
    /// Validation is kept separate to avoid process hang issues with SpinWait
    /// contention when validation runs inside ForceComplete phases.
    /// </summary>
    public override ImmutableArray<Diagnostic> GetDiagnostics(CancellationToken cancellationToken = default)
    {
        var namespaceDiagnostics = SourceGlobalNamespace.DeclarationDiagnostics;
        SourceGlobalNamespace.ForceComplete(cancellationToken);
        var declarationDiagnostics = DeclarationDiagnostics;
        var builder = ImmutableArray.CreateBuilder<Diagnostic>(namespaceDiagnostics.Count + declarationDiagnostics.Count);
        builder.AddRange(namespaceDiagnostics.AsEnumerable());
        builder.AddRange(declarationDiagnostics.AsEnumerable());
        return builder.MoveToImmutable();
    }

    /// <summary>
    /// Runs constraint validation on all rosters in the compilation and returns
    /// validation diagnostics. This is separate from <see cref="GetDiagnostics"/>
    /// to allow callers to get validation results without triggering full symbol completion.
    /// </summary>
    /// <param name="forceCatalogues">
    /// Optional list of catalogues corresponding to each force in the roster,
    /// in the same order as <see cref="RosterNode.Forces"/>. When provided, these
    /// override the catalogue lookup from <see cref="ForceNode.CatalogueId"/>.
    /// This is needed because force entries often live in the gamesystem while
    /// selection entries live in separate catalogues.
    /// </param>
    /// <remarks>
    /// Currently assumes a single roster per compilation. The <paramref name="forceCatalogues"/>
    /// override applies to all rosters equally, which is incorrect for multi-roster
    /// compilations. This will need refactoring if multi-roster support is added.
    /// </remarks>
    public ImmutableArray<Diagnostic> GetValidationDiagnostics(
        IReadOnlyList<ICatalogueSymbol>? forceCatalogues = null)
    {
        var bag = DiagnosticBag.GetInstance();
        foreach (var roster in SourceGlobalNamespace.Rosters)
        {
            ConstraintValidator.Validate(roster.Declaration, this, bag, forceCatalogues);
        }
        var result = bag.ToReadOnlyAndFree();
        return result;
    }

    public override WhamCompilation AddSourceTrees(params SourceTree[] trees) =>
        Update(SourceTrees.AddRange(trees));

    public override WhamCompilation ReplaceSourceTree(SourceTree oldTree, SourceTree? newTree) =>
        Update(newTree is null ? SourceTrees.Remove(oldTree) : SourceTrees.Replace(oldTree, newTree));

    private WhamCompilation Update(ImmutableArray<SourceTree> trees) =>
        new(Name, trees, Options);

    internal override ICatalogueSymbol CreateMissingGamesystemSymbol(DiagnosticBag diagnostics)
    {
        diagnostics.Add(ErrorCode.ERR_MissingGamesystem, Location.None);
        return new ErrorSymbols.ErrorGamesystemSymbol();
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
}
