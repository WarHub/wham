using WarHub.ArmouryModel.Source;

namespace WarHub.ArmouryModel.Concrete;

[GenerateSymbol(SymbolKind.CatalogueReference)]
internal sealed partial class CatalogueReferenceSymbol : SourceDeclaredSymbol, ICatalogueReferenceSymbol, INodeDeclaredSymbol<CatalogueLinkNode>
{
    private ICatalogueSymbol? lazyCatalogue;

    public CatalogueReferenceSymbol(ICatalogueSymbol containingSymbol, CatalogueLinkNode declaration)
        : base(containingSymbol, declaration)
    {
        Declaration = declaration;
    }

    public bool ImportsRootEntries => Declaration.ImportRootEntries;

    public int CatalogueRevision => default;

    [Bound]
    public ICatalogueSymbol Catalogue =>
        GetBoundField(ref lazyCatalogue, Declaration, static (b, d, decl) => b.BindCatalogueSymbol(decl, d));

    public override CatalogueLinkNode Declaration { get; }
}
