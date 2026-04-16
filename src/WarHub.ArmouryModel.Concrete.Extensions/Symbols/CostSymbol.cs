using WarHub.ArmouryModel.Source;

namespace WarHub.ArmouryModel.Concrete;

internal sealed class CostSymbol : ResourceEntryBaseSymbol, ICostSymbol, INodeDeclaredSymbol<CostNode>
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

    public override IResourceDefinitionSymbol Type =>
        GetBoundField(ref lazyTypeSymbol, (b, d) => b.BindCostTypeSymbol(Declaration, d));

    protected override void CheckReferencesCore() => _ = Type;

    public decimal Value => Declaration.Value;
}
