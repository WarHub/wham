using WarHub.ArmouryModel.Source;

namespace WarHub.ArmouryModel.Concrete;

[GenerateSymbol(SymbolKind.RosterCost)]
internal sealed partial class RosterCostSymbol : SourceDeclaredSymbol, IRosterCostSymbol
{
    private IResourceDefinitionSymbol? lazyType;

    public RosterCostSymbol(
        ISymbol? containingSymbol,
        CostNode costDeclaration,
        CostLimitNode? limitDeclaration,
        DiagnosticBag diagnostics)
        : base(containingSymbol, costDeclaration)
    {
        CostDeclaration = costDeclaration;
        LimitDeclaration = limitDeclaration; // TODO consider nesting another symbol, or how to declare multiple IDs.
        if (limitDeclaration?.TypeId is { } typeId && typeId != costDeclaration.TypeId)
        {
            diagnostics.Add(
                ErrorCode.ERR_GenericError,
                costDeclaration.GetLocation(),
                symbols: ImmutableArray.Create<Symbol>(this),
                args: "Cost limit has a different TypeId than Cost value.");
        }
    }

    public CostNode CostDeclaration { get; }

    public CostLimitNode? LimitDeclaration { get; }

    public decimal Value => CostDeclaration.Value;

    public decimal? Limit
    {
        get
        {
            // explainer: BS behavior is that a -1 limit value represents absence of limit
            return LimitDeclaration is { Value: var val and >= 0 } ? val : null;
        }
    }

    [Bound]
    public IResourceDefinitionSymbol CostType =>
        GetBoundField(ref lazyType, CostDeclaration, static (b, d, decl) => b.BindCostTypeSymbol(decl, d));
}
