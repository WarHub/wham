using WarHub.ArmouryModel.Source;

namespace WarHub.ArmouryModel.Concrete;

[GenerateSymbol(SymbolKind.Constraint)]
internal sealed partial class ConstraintSymbol : LogicBaseSymbol, IConstraintSymbol, INodeDeclaredSymbol<ConstraintNode>
{
    public ConstraintSymbol(
        ISymbol? containingSymbol,
        ConstraintNode declaration,
        DiagnosticBag diagnostics)
        : base(containingSymbol, declaration)
    {
        Declaration = declaration;
        Query = QueryBaseSymbol.Create(this, declaration, diagnostics);
    }

    public new ConstraintNode Declaration { get; }

    public QueryBaseSymbol Query { get; }

    IQuerySymbol IConstraintSymbol.Query => Query;

    protected override ImmutableArray<Symbol> MakeAllMembers(BindingDiagnosticBag diagnostics) =>
        base.MakeAllMembers(diagnostics)
        .Add(Query);
}
