using WarHub.ArmouryModel.Source;

namespace WarHub.ArmouryModel.Concrete;

internal sealed class RosterSymbol : SourceDeclaredSymbol, IRosterSymbol, INodeDeclaredSymbol<RosterNode>
{
    private ICatalogueSymbol? lazyGamesystem;
    private EffectiveEntryCache? effectiveEntryCache;

    public RosterSymbol(
        SourceGlobalNamespaceSymbol containingSymbol,
        RosterNode declaration,
        DiagnosticBag diagnostics) : base(containingSymbol, declaration)
    {
        ContainingNamespace = containingSymbol;
        Declaration = declaration;
        Costs = CreateCosts().ToImmutableArray();
        Forces = declaration.Forces.Select(x => new ForceSymbol(this, x, diagnostics)).ToImmutableArray();

        IEnumerable<RosterCostSymbol> CreateCosts()
        {
            foreach (var cost in declaration.Costs)
            {
                var limits = declaration.CostLimits.Where(x => x.TypeId == cost.TypeId).ToList();
                if (limits.Count > 1)
                {
                    diagnostics.Add(
                        ErrorCode.ERR_GenericError,
                        cost.GetLocation(),
                        symbols: ImmutableArray.Create<Symbol>(this),
                        args: "There are multiple Cost Limits with the TypeId of this Cost value.");
                }
                var limit = limits.FirstOrDefault();
                yield return new RosterCostSymbol(this, cost, limit, diagnostics);
            }
        }
    }

    public override RosterNode Declaration { get; }

    public override SourceGlobalNamespaceSymbol ContainingNamespace { get; }

    public override IModuleSymbol? ContainingModule => null;

    public override SymbolKind Kind => SymbolKind.Roster;

    public string? CustomNotes => Declaration.CustomNotes;

    public ICatalogueSymbol Gamesystem => GetBoundField(ref lazyGamesystem);

    /// <summary>
    /// Gets or lazily creates the effective entry cache for this roster.
    /// The cache uses an internal <see cref="ModifierEvaluator"/> to compute
    /// effective values on first access. Thread-safe (CAS-protected, set-once).
    /// </summary>
    internal EffectiveEntryCache GetOrCreateEffectiveEntryCache()
    {
        if (effectiveEntryCache is { } existing)
            return existing;
        var cache = new EffectiveEntryCache(Declaration, (WhamCompilation)DeclaringCompilation);
        Interlocked.CompareExchange(ref effectiveEntryCache, cache, null);
        return effectiveEntryCache!;
    }

    public ImmutableArray<RosterCostSymbol> Costs { get; }

    public ImmutableArray<ForceSymbol> Forces { get; }

    ImmutableArray<IRosterCostSymbol> IRosterSymbol.Costs =>
        Costs.Cast<RosterCostSymbol, IRosterCostSymbol>();

    ImmutableArray<IForceSymbol> IForceContainerSymbol.Forces =>
        Forces.Cast<ForceSymbol, IForceSymbol>();

    public ISelectionEntryContainerSymbol GetEffectiveEntry(
        ISelectionEntryContainerSymbol declaredEntry,
        ISelectionSymbol? selection = null,
        IForceSymbol? force = null)
    {
        var cache = GetOrCreateEffectiveEntryCache();
        var selNode = (selection as SelectionSymbol)?.Declaration;
        var forceNode = (force as ForceSymbol)?.Declaration;
        return cache.GetEffectiveEntry(declaredEntry, selNode, forceNode);
    }

    public override void Accept(SymbolVisitor visitor) =>
        visitor.VisitRoster(this);

    public override TResult Accept<TResult>(SymbolVisitor<TResult> visitor) =>
        visitor.VisitRoster(this);

    public override TResult Accept<TArgument, TResult>(SymbolVisitor<TArgument, TResult> visitor, TArgument argument) =>
        visitor.VisitRoster(this, argument);

    protected override void BindReferencesCore(Binder binder, BindingDiagnosticBag diagnostics)
    {
        base.BindReferencesCore(binder, diagnostics);
        lazyGamesystem = binder.BindGamesystemSymbol(Declaration, diagnostics);
    }

    protected override ImmutableArray<Symbol> MakeAllMembers(BindingDiagnosticBag diagnostics) =>
        base.MakeAllMembers(diagnostics)
        .AddRange(Costs.Cast<RosterCostSymbol, Symbol>())
        .AddRange(Forces.Cast<ForceSymbol, Symbol>());
}
