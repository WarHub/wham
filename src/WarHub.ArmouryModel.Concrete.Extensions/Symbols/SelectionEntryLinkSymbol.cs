using WarHub.ArmouryModel.Source;

namespace WarHub.ArmouryModel.Concrete;

internal sealed partial class SelectionEntryLinkSymbol : SelectionEntryBaseSymbol, INodeDeclaredSymbol<EntryLinkNode>
{
    private ISelectionEntryContainerSymbol? lazyReference;

    public SelectionEntryLinkSymbol(
        ISymbol containingSymbol,
        EntryLinkNode declaration,
        DiagnosticBag diagnostics)
        : base(containingSymbol, declaration, diagnostics)
    {
        Declaration = declaration;
        ContainerKind = declaration.Type switch
        {
            EntryLinkKind.SelectionEntry => ContainerKind.Selection,
            EntryLinkKind.SelectionEntryGroup => ContainerKind.SelectionGroup,
            _ => ContainerKind.Error,
        };
        if (ContainerKind is ContainerKind.Error)
        {
            diagnostics.Add(
                ErrorCode.ERR_UnknownEnumerationValue,
                declaration.GetLocation(),
                declaration.Type);
        }
    }

    public override EntryLinkNode Declaration { get; }

    public override ContainerKind ContainerKind { get; }

    [Bound]
    public override ISelectionEntryContainerSymbol ReferencedEntry =>
        GetBoundField(ref lazyReference, Declaration, static (b, d, decl) => b.BindSelectionEntrySymbol(decl, d));
}
