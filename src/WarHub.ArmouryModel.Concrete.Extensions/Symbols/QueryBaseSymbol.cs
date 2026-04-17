using WarHub.ArmouryModel.Source;

namespace WarHub.ArmouryModel.Concrete;

[GenerateSymbol(SymbolKind.Query)]
internal abstract partial class QueryBaseSymbol : LogicBaseSymbol, IQuerySymbol
{
    private ISymbol? lazyValueType;
    private ISymbol? lazyScope;
    private ISymbol? lazyFilter;

    public QueryBaseSymbol(
        ISymbol containingSymbol,
        QueryBaseNode declaration,
        DiagnosticBag diagnostics)
        : base(containingSymbol, declaration)
    {
        Declaration = declaration;
        ValueKind = declaration.Field switch
        {
            "forces" => QueryValueKind.ForceCount,
            "selections" => QueryValueKind.SelectionCount,
            { } id when LimitField.IsMatch(id) => QueryValueKind.MemberValueLimit,
            { } id when !string.IsNullOrWhiteSpace(id) => QueryValueKind.MemberValue,
            _ => QueryValueKind.Unknown
        };
        if (ValueKind is QueryValueKind.Unknown)
        {
            diagnostics.Add(
                ErrorCode.ERR_UnknownEnumerationValue,
                declaration.GetLocation(),
                declaration.Field ?? "field");
        }

        ScopeKind = declaration.Scope switch
        {
            "self" => QueryScopeKind.Self,
            "parent" => QueryScopeKind.Parent,
            "ancestor" => QueryScopeKind.ContainingAncestor,
            "primary-category" => QueryScopeKind.PrimaryCategory,
            "primary-catalogue" => QueryScopeKind.PrimaryCatalogue,
            "force" => QueryScopeKind.ContainingForce,
            "roster" => QueryScopeKind.ContainingRoster,
            { } id when !string.IsNullOrWhiteSpace(id) => QueryScopeKind.ReferencedEntry,
            _ => QueryScopeKind.Unknown
        };
        if (ValueKind is QueryValueKind.Unknown)
        {
            diagnostics.Add(
                ErrorCode.ERR_UnknownEnumerationValue,
                declaration.GetLocation(),
                declaration.Scope ?? "scope");
        }

        ValueFilterKind = declaration is QueryFilteredBaseNode { } filtered
            ? filtered.ChildId switch
            {
                "any" => QueryFilterKind.Anything,
                "unit" => QueryFilterKind.UnitEntry,
                "model" => QueryFilterKind.ModelEntry,
                "upgrade" => QueryFilterKind.UpgradeEntry,
                { } id when !string.IsNullOrWhiteSpace(id) => QueryFilterKind.SpecifiedEntry,
                _ => QueryFilterKind.Unknown
            }
            : QueryFilterKind.Anything;
        if (ValueFilterKind is QueryFilterKind.Unknown)
        {
            diagnostics.Add(
                ErrorCode.ERR_UnknownEnumerationValue,
                declaration.GetLocation(),
                (declaration as QueryFilteredBaseNode)?.ChildId ?? "childId");
        }
        Options = CreateOptions();
        // TODO more diagnostics

        QueryOptions CreateOptions()
        {
            var options = QueryOptions.None;
            if (declaration.Shared)
            {
                options |= QueryOptions.SharedConstraint;
            }
            if (declaration.IncludeChildForces)
            {
                options |= QueryOptions.IncludeDescendantForces;
            }
            if (declaration.IncludeChildSelections)
            {
                options |= QueryOptions.IncludeDescendantSelections;
            }
            if (declaration.IsValuePercentage)
            {
                options |= QueryOptions.ValuePercentage;
            }
            if (declaration is RepeatNode { RoundUp: true })
            {
                options |= QueryOptions.ValueRoundUp;
            }
            return options;
        }
    }

    public new QueryBaseNode Declaration { get; }

    public abstract QueryComparisonType Comparison { get; }

    public decimal? ReferenceValue => Declaration.Value;

    public QueryValueKind ValueKind { get; }

    [Bound]
    public ISymbol? ValueTypeSymbol
    {
        get
        {
            if (ValueKind is QueryValueKind.MemberValue)
                return GetBoundField(ref lazyValueType, Declaration, static (b, d, decl) => b.BindCostTypeSymbol(decl, decl.Field, d));
            if (ValueKind is QueryValueKind.MemberValueLimit)
                return GetBoundField(ref lazyValueType, Declaration, static (b, d, decl) => b.BindCostTypeSymbol(decl, LimitField.GetCostTypeId(decl.Field), d));
            return null;
        }
    }

    public QueryScopeKind ScopeKind { get; }

    [Bound]
    public ISymbol? ScopeSymbol
    {
        get
        {
            if (ScopeKind is QueryScopeKind.ReferencedEntry)
                return GetBoundField(ref lazyScope, Declaration, static (b, d, decl) => b.BindScopeEntrySymbol(decl, decl.Scope, d));
            return null;
        }
    }

    public QueryFilterKind ValueFilterKind { get; }

    [Bound]
    public ISymbol? FilterSymbol
    {
        get
        {
            if (ValueFilterKind is QueryFilterKind.SpecifiedEntry)
                return GetBoundField(ref lazyFilter, this, static (b, d, self) => b.BindFilterEntrySymbol(self.Declaration, ((QueryFilteredBaseNode)self.Declaration).ChildId, self.ScopeKind, d));
            return null;
        }
    }

    public QueryOptions Options { get; }

    public static QueryBaseSymbol Create(ISymbol containingSymbol, QueryBaseNode declaration, DiagnosticBag diagnostics)
    {
        return declaration switch
        {
            ConditionNode x => new ConditionQuerySymbol(containingSymbol, x, diagnostics),
            ConstraintNode x => new ConstraintQuerySymbol(containingSymbol, x, diagnostics),
            RepeatNode x => new RepeatQuerySymbol(containingSymbol, x, diagnostics),
            _ => throw new InvalidOperationException("Unsupported declaration type.")
        };
    }

    internal sealed class ConditionQuerySymbol : QueryBaseSymbol
    {
        public ConditionQuerySymbol(
            ISymbol containingSymbol,
            ConditionNode declaration,
            DiagnosticBag diagnostics)
            : base(containingSymbol, declaration, diagnostics)
        {
            Comparison = declaration.Type switch
            {
                ConditionKind.EqualTo => QueryComparisonType.Equal,
                ConditionKind.LessThan => QueryComparisonType.LessThan,
                ConditionKind.GreaterThan => QueryComparisonType.GreaterThan,
                ConditionKind.NotEqualTo => QueryComparisonType.NotEqual,
                ConditionKind.AtLeast => QueryComparisonType.GreaterThanOrEqual,
                ConditionKind.AtMost => QueryComparisonType.LessThanOrEqual,
                ConditionKind.InstanceOf => QueryComparisonType.InstanceOf,
                ConditionKind.NotInstanceOf => QueryComparisonType.NotInstanceOf,
                _ => QueryComparisonType.None
            };
            if (Comparison is QueryComparisonType.None)
                diagnostics.Add(
                    ErrorCode.ERR_UnknownEnumerationValue,
                    declaration.GetLocation(),
                    declaration.Type);
        }

        public override QueryComparisonType Comparison { get; }
    }

    internal sealed class ConstraintQuerySymbol : QueryBaseSymbol
    {
        public ConstraintQuerySymbol(
            ISymbol containingSymbol,
            ConstraintNode declaration,
            DiagnosticBag diagnostics)
            : base(containingSymbol, declaration, diagnostics)
        {
            Comparison = declaration.Type switch
            {
                ConstraintKind.Minimum => QueryComparisonType.GreaterThanOrEqual,
                ConstraintKind.Maximum => QueryComparisonType.LessThanOrEqual,
                _ => QueryComparisonType.None
            };
            if (Comparison is QueryComparisonType.None)
                diagnostics.Add(
                    ErrorCode.ERR_UnknownEnumerationValue,
                    declaration.GetLocation(),
                    declaration.Type);
        }

        public override QueryComparisonType Comparison { get; }
    }

    internal sealed class RepeatQuerySymbol : QueryBaseSymbol
    {
        public RepeatQuerySymbol(
            ISymbol containingSymbol,
            QueryBaseNode declaration,
            DiagnosticBag diagnostics)
            : base(containingSymbol, declaration, diagnostics)
        {
        }

        public override QueryComparisonType Comparison => QueryComparisonType.None;
    }

    private static class LimitField
    {
        private const string Prefix = "limit::";

        public static bool IsMatch(string field) =>
            field.StartsWith(Prefix, StringComparison.Ordinal);

        public static string? GetCostTypeId(string? field) =>
            field?[Prefix.Length..];
    }
}
