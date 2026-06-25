using WarHub.ArmouryModel.Source;

namespace WarHub.ArmouryModel.Concrete;

internal sealed partial class CostSymbol : ResourceEntryBaseSymbol, ICostSymbol, INodeDeclaredSymbol<CostNode>
{
    private IResourceDefinitionSymbol? lazyTypeSymbol;

    public CostSymbol(
        ISymbol containingSymbol,
        CostNode declaration,
        DiagnosticBag diagnostics)
        : base(containingSymbol, declaration, diagnostics)
    {
        Declaration = declaration;
    }

    public override CostNode Declaration { get; }

    public override ResourceKind ResourceKind => ResourceKind.Cost;

    [Bound]
    public override IResourceDefinitionSymbol Type =>
        GetBoundField(ref lazyTypeSymbol, Declaration, static (b, d, decl) => b.BindCostTypeSymbol(decl, d));

    public decimal Value => Declaration.Value;
}
