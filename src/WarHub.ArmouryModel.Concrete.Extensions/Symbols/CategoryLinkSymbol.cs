using WarHub.ArmouryModel.Source;

namespace WarHub.ArmouryModel.Concrete;

internal sealed partial class CategoryLinkSymbol : ContainerEntryBaseSymbol, ICategoryEntrySymbol, INodeDeclaredSymbol<CategoryLinkNode>
{
    private ICategoryEntrySymbol? lazyReference;

    public CategoryLinkSymbol(
        ISymbol containingSymbol,
        CategoryLinkNode declaration,
        DiagnosticBag diagnostics)
        : base(containingSymbol, declaration, diagnostics)
    {
        Declaration = declaration;
    }

    public override ContainerKind ContainerKind => ContainerKind.Category;

    public bool IsPrimaryCategory => Declaration.Primary;

    [Bound]
    public override ICategoryEntrySymbol ReferencedEntry =>
        GetBoundField(ref lazyReference, Declaration, static (b, d, decl) => b.BindCategoryEntrySymbol(decl, d));

    public override CategoryLinkNode Declaration { get; }
}
