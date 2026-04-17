using WarHub.ArmouryModel.Source;

namespace WarHub.ArmouryModel.Concrete;

internal sealed class CatalogueSymbol : CatalogueBaseSymbol, INodeDeclaredSymbol<CatalogueNode>
{
    private ICatalogueSymbol? lazyGamesystem;

    public CatalogueSymbol(
        SourceGlobalNamespaceSymbol containingSymbol,
        CatalogueNode declaration,
        DiagnosticBag diagnostics)
        : base(containingSymbol, declaration, diagnostics)
    {
        Declaration = declaration;
        CatalogueReferences = CreateLinks().ToImmutableArray();

        IEnumerable<CatalogueReferenceSymbol> CreateLinks()
        {
            foreach (var item in declaration.CatalogueLinks)
            {
                yield return new CatalogueReferenceSymbol(this, item);
            }
        }
    }

    public override CatalogueNode Declaration { get; }

    public override bool IsLibrary => Declaration.IsLibrary;

    public override bool IsGamesystem => false;

    public override ICatalogueSymbol Gamesystem =>
        GetBoundField(ref lazyGamesystem, Declaration, static (b, d, decl) => b.BindGamesystemSymbol(decl, d));

    protected override void CheckReferencesCore() => _ = Gamesystem;

    public override ImmutableArray<CatalogueReferenceSymbol> CatalogueReferences { get; }

    CatalogueNode INodeDeclaredSymbol<CatalogueNode>.Declaration => Declaration;
}
