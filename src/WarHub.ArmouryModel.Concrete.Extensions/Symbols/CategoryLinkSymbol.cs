using WarHub.ArmouryModel.Source;

namespace WarHub.ArmouryModel.Concrete;

internal sealed class CategoryLinkSymbol : ContainerEntryBaseSymbol, ICategoryEntrySymbol, INodeDeclaredSymbol<CategoryLinkNode>
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

    public override ICategoryEntrySymbol ReferencedEntry =>
        GetBoundField(ref lazyReference, (b, d) => b.BindCategoryEntrySymbol(Declaration, d));

    protected override void CheckReferencesCore() => _ = ReferencedEntry;

    public override CategoryLinkNode Declaration { get; }
}
