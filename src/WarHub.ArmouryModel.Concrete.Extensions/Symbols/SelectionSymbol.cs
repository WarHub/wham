using WarHub.ArmouryModel.Source;

namespace WarHub.ArmouryModel.Concrete;

internal sealed class SelectionSymbol : ContainerSymbol, ISelectionSymbol, INodeDeclaredSymbol<SelectionNode>
{
    private ISelectionEntryContainerSymbol? lazyEffectiveSourceEntry;

    public SelectionSymbol(
        ISymbol? containingSymbol,
        SelectionNode declaration,
        DiagnosticBag diagnostics)
        : base(containingSymbol, declaration, diagnostics)
    {
        Declaration = declaration;
        Costs = declaration.Costs.Select(x => new CostSymbol(this, x, diagnostics)).ToImmutableArray();
        Categories = declaration.Categories.Select(x => new CategorySymbol(this, x, diagnostics)).ToImmutableArray();
        ChildSelections = declaration.Selections.Select(x => new SelectionSymbol(this, x, diagnostics)).ToImmutableArray();
        PrimaryCategory = Categories.FirstOrDefault(x => x.IsPrimaryCategory); // TODO diagnostic if count != 1 for root selection?
    }

    public new SelectionNode Declaration { get; }

    public override ContainerKind ContainerKind => ContainerKind.Selection;

    public int SelectedCount => Declaration.Number;

    public override ISelectionEntrySymbol SourceEntry =>
        (ISelectionEntrySymbol)SourceEntryPath.SourceEntries.Last();

    public ISelectionEntryContainerSymbol EffectiveSourceEntry
    {
        get
        {
            if (lazyEffectiveSourceEntry is { } cached)
                return cached;
            var roster = GetRosterSymbol();
            var result = roster?.GetOrCreateEffectiveEntryCache().GetEffectiveEntry(
                SourceEntry,
                this,
                GetContainingForce())
                ?? (ISelectionEntryContainerSymbol)SourceEntry;
            Interlocked.CompareExchange(ref lazyEffectiveSourceEntry, result, null);
            return lazyEffectiveSourceEntry;
        }
    }

    private IForceSymbol? GetContainingForce()
    {
        for (var sym = ContainingSymbol; sym is not null; sym = sym.ContainingSymbol)
        {
            if (sym is IForceSymbol force)
                return force;
        }
        return null;
    }

    public ImmutableArray<SelectionSymbol> ChildSelections { get; }

    public SelectionEntryKind EntryKind => Declaration.Type;

    public ICategorySymbol? PrimaryCategory { get; }

    public ImmutableArray<CategorySymbol> Categories { get; }

    public ImmutableArray<CostSymbol> Costs { get; }

    ImmutableArray<ICategorySymbol> ISelectionSymbol.Categories =>
        Categories.Cast<CategorySymbol, ICategorySymbol>();

    ImmutableArray<ICostSymbol> ISelectionSymbol.Costs =>
        Costs.Cast<CostSymbol, ICostSymbol>();

    ImmutableArray<ISelectionSymbol> ISelectionContainerSymbol.Selections =>
        ChildSelections.Cast<SelectionSymbol, ISelectionSymbol>();

    protected override ImmutableArray<Symbol> MakeAllMembers(BindingDiagnosticBag diagnostics) =>
        base.MakeAllMembers(diagnostics)
        .AddRange(Costs.Cast<CostSymbol, Symbol>())
        .AddRange(Categories.Cast<CategorySymbol, Symbol>())
        .AddRange(ChildSelections.Cast<SelectionSymbol, Symbol>());
}
