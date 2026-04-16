using WarHub.ArmouryModel.Source;

namespace WarHub.ArmouryModel.Concrete;

internal sealed class SelectionEntryGroupSymbol : SelectionEntryBaseSymbol, ISelectionEntryGroupSymbol, INodeDeclaredSymbol<SelectionEntryGroupNode>
{
    private ISelectionEntryContainerSymbol? lazyDefaultEntry;

    public SelectionEntryGroupSymbol(
        ISymbol containingSymbol,
        SelectionEntryGroupNode declaration,
        DiagnosticBag diagnostics)
        : base(containingSymbol, declaration, diagnostics)
    {
        Declaration = declaration;
    }

    public override SelectionEntryGroupNode Declaration { get; }

    public override ContainerKind ContainerKind => ContainerKind.SelectionGroup;

    public ISelectionEntryContainerSymbol? DefaultSelectionEntry
    {
        get
        {
            if (Declaration.DefaultSelectionEntryId is not null)
                return GetBoundField(ref lazyDefaultEntry, (b, d) => b.BindSelectionEntryGroupDefaultEntrySymbol(Declaration, this, d));
            return null;
        }
    }

    protected override void CheckReferencesCore() => _ = DefaultSelectionEntry;
}
