using WarHub.ArmouryModel.Source;

namespace WarHub.ArmouryModel.Concrete;

[GenerateSymbol(SymbolKind.Condition)]
internal abstract partial class ConditionBaseSymbol : LogicBaseSymbol, IConditionSymbol
{
    protected ConditionBaseSymbol(
        ISymbol containingSymbol,
        SourceNode declaration)
        : base(containingSymbol, declaration)
    {
    }

    public abstract QueryBaseSymbol? Query { get; }

    public abstract LogicalOperator ChildrenOperator { get; }

    public abstract ImmutableArray<ConditionBaseSymbol> Children { get; }

    IQuerySymbol? IConditionSymbol.Query => Query;

    ImmutableArray<IConditionSymbol> IConditionSymbol.Children =>
        Children.Cast<ConditionBaseSymbol, IConditionSymbol>();

    protected override ImmutableArray<Symbol> MakeAllMembers(BindingDiagnosticBag diagnostics) =>
        base.MakeAllMembers(diagnostics)
        .AddWhenNotNull(Query)
        .AddRange(Children.Cast<ConditionBaseSymbol, Symbol>());
}
