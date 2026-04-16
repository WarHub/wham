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

    public ICatalogueSymbol Gamesystem =>
        GetBoundField(ref lazyGamesystem, (b, d) => b.BindGamesystemSymbol(Declaration, d));

    protected override void CheckReferencesCore() => _ = Gamesystem;

    /// <summary>
    /// Gets or lazily creates the effective entry cache for this roster.
    /// The cache uses an internal <see cref="ModifierEvaluator"/> to compute
    /// effective values on first access. Thread-safe (CAS-protected, set-once).
    /// </summary>
    internal EffectiveEntryCache GetOrCreateEffectiveEntryCache()
    {
        if (effectiveEntryCache is { } existing)
            return existing;
        var cache = new EffectiveEntryCache(this, (WhamCompilation)DeclaringCompilation);
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
        return cache.GetEffectiveEntry(declaredEntry, selection, force);
    }

    public override void Accept(SymbolVisitor visitor) =>
        visitor.VisitRoster(this);

    public override TResult Accept<TResult>(SymbolVisitor<TResult> visitor) =>
        visitor.VisitRoster(this);

    public override TResult Accept<TArgument, TResult>(SymbolVisitor<TArgument, TResult> visitor, TArgument argument) =>
        visitor.VisitRoster(this, argument);

    protected override void ComputeEffectiveEntries()
    {
        if (state.HasComplete(CompletionPart.EffectiveEntriesCompleted))
            return;
        if (state.NotePartComplete(CompletionPart.StartEffectiveEntries))
        {
            EnsureReferencedCataloguesComplete();
            var cache = GetOrCreateEffectiveEntryCache();
            foreach (var force in Forces)
            {
                PopulateForceEffectiveEntries(cache, force);
            }
            state.NotePartComplete(CompletionPart.FinishEffectiveEntries);
        }
        state.SpinWaitComplete(CompletionPart.EffectiveEntriesCompleted, default);
    }

    private void EnsureReferencedCataloguesComplete()
    {
        // Force-complete the Gamesystem catalogue
        if (Gamesystem is SourceDeclaredSymbol gamesystem)
        {
            gamesystem.ForceComplete(default);
        }
        // Force-complete each force's referenced catalogue
        foreach (var force in Forces)
        {
            EnsureForcesCataloguesComplete(force);
        }
    }

    private static void EnsureForcesCataloguesComplete(ForceSymbol force)
    {
        if (force.CatalogueReference.Catalogue is SourceDeclaredSymbol catalogue)
        {
            catalogue.ForceComplete(default);
        }
        foreach (var childForce in force.Forces)
        {
            EnsureForcesCataloguesComplete(childForce);
        }
    }

    private static void PopulateForceEffectiveEntries(EffectiveEntryCache cache, ForceSymbol force)
    {
        foreach (var selection in force.ChildSelections)
        {
            PopulateSelectionEffectiveEntries(cache, selection, force);
        }
        foreach (var childForce in force.Forces)
        {
            PopulateForceEffectiveEntries(cache, childForce);
        }
    }

    private static void PopulateSelectionEffectiveEntries(
        EffectiveEntryCache cache, SelectionSymbol selection, ForceSymbol force)
    {
        var effective = cache.GetEffectiveEntry(selection.SourceEntry, selection, force);
        Interlocked.CompareExchange(ref selection.lazyEffectiveSourceEntry, effective, null);
        foreach (var child in selection.ChildSelections)
        {
            PopulateSelectionEffectiveEntries(cache, child, force);
        }
    }

    protected override void CheckConstraints()
    {
        if (state.HasComplete(CompletionPart.CheckConstraintsCompleted))
            return;
        if (state.NotePartComplete(CompletionPart.StartCheckConstraints))
        {
            var diagnostics = DiagnosticBag.GetInstance();
            ConstraintEvaluator.Evaluate(this, (WhamCompilation)DeclaringCompilation, diagnostics);
            AddConstraintDiagnostics(diagnostics);
            diagnostics.Free();
            state.NotePartComplete(CompletionPart.FinishCheckConstraints);
        }
        state.SpinWaitComplete(CompletionPart.CheckConstraintsCompleted, default);
    }

    protected override ImmutableArray<Symbol> MakeAllMembers(BindingDiagnosticBag diagnostics) =>
        base.MakeAllMembers(diagnostics)
        .AddRange(Costs.Cast<RosterCostSymbol, Symbol>())
        .AddRange(Forces.Cast<ForceSymbol, Symbol>());
}
