using WarHub.ArmouryModel.Source;

namespace WarHub.ArmouryModel.Concrete;

internal sealed class ResourceLinkSymbol : ResourceEntryBaseSymbol, IResourceEntrySymbol, INodeDeclaredSymbol<InfoLinkNode>
{
    private IResourceEntrySymbol? lazyReferencedEntry;

    public ResourceLinkSymbol(
        ISymbol containingSymbol,
        InfoLinkNode declaration,
        DiagnosticBag diagnostics)
        : base(containingSymbol, declaration, diagnostics)
    {
        Declaration = declaration;
        ResourceKind = declaration.Type switch
        {
            InfoLinkKind.InfoGroup => ResourceKind.Group,
            InfoLinkKind.Profile => ResourceKind.Profile,
            InfoLinkKind.Rule => ResourceKind.Rule,
            _ => ResourceKind.Error,
        };
        if (ResourceKind is ResourceKind.Error)
        {
            diagnostics.Add(
                ErrorCode.ERR_UnknownEnumerationValue,
                declaration.GetLocation(),
                declaration.Type);
        }
    }

    public override InfoLinkNode Declaration { get; }

    public override ResourceKind ResourceKind { get; }

    public override IResourceEntrySymbol ReferencedEntry =>
        GetBoundField(ref lazyReferencedEntry, Declaration, static (b, d, decl) => b.BindSharedResourceEntrySymbol(decl, d));

    protected override void CheckReferencesCore() => _ = ReferencedEntry;
}
