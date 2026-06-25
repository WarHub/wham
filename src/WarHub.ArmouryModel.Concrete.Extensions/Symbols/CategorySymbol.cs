using WarHub.ArmouryModel.Source;

namespace WarHub.ArmouryModel.Concrete;

internal sealed partial class CategorySymbol : ContainerSymbol, ICategorySymbol, INodeDeclaredSymbol<CategoryNode>
{
    private ICategoryEntrySymbol? lazyCategoryEntry;

    public CategorySymbol(
        ISymbol? containingSymbol,
        CategoryNode declaration,
        DiagnosticBag diagnostics)
        : base(containingSymbol, declaration, diagnostics)
    {
        Declaration = declaration;
    }

    public new CategoryNode Declaration { get; }

    [Bound]
    public override ICategoryEntrySymbol SourceEntry =>
        GetBoundField(ref lazyCategoryEntry, Declaration, static (b, d, decl) => b.BindCategoryEntrySymbol(decl, d));

    public override ContainerKind ContainerKind => ContainerKind.Category;

    public bool IsPrimaryCategory => Declaration.Primary;
}
