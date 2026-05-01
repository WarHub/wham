using WarHub.ArmouryModel.Source;

namespace WarHub.ArmouryModel.Concrete;

internal sealed partial class SelectionEntryGroupSymbol : SelectionEntryBaseSymbol, ISelectionEntryGroupSymbol, INodeDeclaredSymbol<SelectionEntryGroupNode>
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

    [Bound]
    public ISelectionEntryContainerSymbol? DefaultSelectionEntry
    {
        get
        {
            if (Declaration.DefaultSelectionEntryId is not null)
                return GetBoundField(ref lazyDefaultEntry, this, static (b, d, self) => b.BindSelectionEntryGroupDefaultEntrySymbol(self.Declaration, self, d));
            return null;
        }
    }
}
