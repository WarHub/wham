using WarHub.ArmouryModel.Source;

namespace WarHub.ArmouryModel.Concrete;

internal sealed class CatalogueReferenceSymbol : SourceDeclaredSymbol, ICatalogueReferenceSymbol, INodeDeclaredSymbol<CatalogueLinkNode>
{
    private ICatalogueSymbol? lazyCatalogue;

    public CatalogueReferenceSymbol(ICatalogueSymbol containingSymbol, CatalogueLinkNode declaration)
        : base(containingSymbol, declaration)
    {
        Declaration = declaration;
    }

    public override SymbolKind Kind => SymbolKind.Link;

    public bool ImportsRootEntries => Declaration.ImportRootEntries;

    public int CatalogueRevision => default;

    public ICatalogueSymbol Catalogue =>
        GetBoundField(ref lazyCatalogue, (b, d) => b.BindCatalogueSymbol(Declaration, d));

    protected override void CheckReferencesCore() => _ = Catalogue;

    public override CatalogueLinkNode Declaration { get; }

    public override void Accept(SymbolVisitor visitor) =>
        visitor.VisitCatalogueReference(this);

    public override TResult Accept<TResult>(SymbolVisitor<TResult> visitor) =>
        visitor.VisitCatalogueReference(this);

    public override TResult Accept<TArgument, TResult>(SymbolVisitor<TArgument, TResult> visitor, TArgument argument) =>
        visitor.VisitCatalogueReference(this, argument);
}
