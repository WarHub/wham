using WarHub.ArmouryModel.Source;

namespace WarHub.ArmouryModel.Concrete;

[GenerateSymbol(SymbolKind.CatalogueReference)]
internal sealed partial class ForceCatalogueReferenceSymbol : SourceDeclaredSymbol, ICatalogueReferenceSymbol
{
    private ICatalogueSymbol? lazyCatalogue;

    public ForceCatalogueReferenceSymbol(
        Symbol? containingSymbol,
        ForceNode declaration,
        DiagnosticBag diagnostics)
        : base(containingSymbol, declaration)
    {
        Declaration = declaration;
    }

    public override ForceNode Declaration { get; }

    public override string? Id => Declaration.CatalogueId;

    public override string Name => Declaration.CatalogueName ?? string.Empty;

    public int CatalogueRevision => Declaration.CatalogueRevision;

    public bool ImportsRootEntries => false;

    [Bound]
    public ICatalogueSymbol Catalogue =>
        GetBoundField(ref lazyCatalogue, Declaration, static (b, d, decl) => b.BindCatalogueSymbol(decl, d));
}
