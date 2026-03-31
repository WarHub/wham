using BattleScribeSpec.Protocol;

namespace WarHub.ArmouryModel.RosterEngine;

/// <summary>
/// Evaluates modifiers and conditions against the current roster state.
/// Instantiated with game system + forces to provide context for evaluation.
/// </summary>
internal sealed class ModifierEvaluator
{
    private readonly ProtocolGameSystem _gameSystem;
    private readonly List<RosterForce> _forces;

    public ModifierEvaluator(ProtocolGameSystem gameSystem, List<RosterForce> forces)
    {
        _gameSystem = gameSystem;
        _forces = forces;
    }

    // ===== Instance wrapper methods for WhamRosterEngine =====

    private EvalContext CreateContext(ProtocolSelectionEntry entry, RosterSelection? selection, RosterForce? force)
    {
        var effectiveId = selection?.SourceLink?.Id ?? selection?.Entry.Id ?? entry.Id;
        return new EvalContext
        {
            AllForces = _forces,
            Force = force ?? (_forces.Count > 0 ? _forces[0] : null!),
            Selection = selection,
            ParentSelection = null,
            OwnerEntryId = effectiveId,
        };
    }

    public double GetEffectiveConstraintValue(ProtocolConstraint constraint, ProtocolSelectionEntry entry,
        RosterSelection? selection, RosterForce? force)
    {
        var ctx = CreateContext(entry, selection, force);
        var modified = Apply(entry, ctx);
        return modified.ConstraintValues.GetValueOrDefault(constraint.Id, constraint.Value);
    }

    public bool GetEffectiveHidden(ProtocolSelectionEntry entry, RosterSelection? selection, RosterForce? force)
    {
        var ctx = CreateContext(entry, selection, force);
        return Apply(entry, ctx).Hidden;
    }

    public string GetEffectiveName(ProtocolSelectionEntry entry, RosterSelection? selection, RosterForce? force)
    {
        var ctx = CreateContext(entry, selection, force);
        return Apply(entry, ctx).Name;
    }

    public List<CostValue> GetEffectiveCosts(ProtocolSelectionEntry entry, RosterSelection? selection, RosterForce? force)
    {
        var ctx = CreateContext(entry, selection, force);
        return Apply(entry, ctx).Costs;
    }

    public string? GetEffectivePage(ProtocolSelectionEntry entry, RosterSelection? selection, RosterForce? force)
    {
        var ctx = CreateContext(entry, selection, force);
        return Apply(entry, ctx).Page;
    }

    public bool EvaluateConditions(ProtocolModifier mod, ProtocolSelectionEntry entry,
        RosterSelection? selection, RosterForce? force)
    {
        var ctx = CreateContext(entry, selection, force);
        return EvaluateAllConditions(mod.Conditions, mod.ConditionGroups, ctx);
    }

    public bool EvaluateGroupConditions(ProtocolModifierGroup group, ProtocolSelectionEntry entry,
        RosterSelection? selection, RosterForce? force)
    {
        var ctx = CreateContext(entry, selection, force);
        return EvaluateAllConditions(group.Conditions, group.ConditionGroups, ctx);
    }

    public int GetRepeatCount(List<ProtocolRepeat>? repeats, ProtocolSelectionEntry entry,
        RosterSelection? selection, RosterForce? force)
    {
        var ctx = CreateContext(entry, selection, force);
        return CalculateRepeatCount(repeats, ctx);
    }

    public static string ApplyStringModifierStatic(string type, string current, string value, int repeatCount)
    {
        var result = current;
        for (int i = 0; i < repeatCount; i++)
        {
            result = type switch
            {
                "set" => value,
                "append" => string.IsNullOrEmpty(result) ? value : result + " " + value,
                "increment" when double.TryParse(result, System.Globalization.NumberStyles.Any,
                        System.Globalization.CultureInfo.InvariantCulture, out var incBase)
                    && double.TryParse(value, System.Globalization.NumberStyles.Any,
                        System.Globalization.CultureInfo.InvariantCulture, out var incVal)
                    => FormatNumber(incBase + incVal),
                "decrement" when double.TryParse(result, System.Globalization.NumberStyles.Any,
                        System.Globalization.CultureInfo.InvariantCulture, out var decBase)
                    && double.TryParse(value, System.Globalization.NumberStyles.Any,
                        System.Globalization.CultureInfo.InvariantCulture, out var decVal)
                    => FormatNumber(decBase - decVal),
                _ => result,
            };
        }
        return result;
    }

    // ===== Core static evaluation logic =====

    public static ModifiedProperties Apply(ProtocolSelectionEntry entry, EvalContext ctx)
    {
        var props = new ModifiedProperties
        {
            Name = entry.Name,
            Hidden = entry.Hidden,
            Page = entry.Page,
        };
        if (entry.Costs is { Count: > 0 })
            props.Costs = entry.Costs.Select(c => new CostValue(c.Name, c.TypeId, c.Value)).ToList();
        if (entry.Constraints is { Count: > 0 })
            foreach (var c in entry.Constraints)
                props.ConstraintValues[c.Id] = c.Value;

        ApplyModifiers(entry.Modifiers, props, ctx);
        ApplyModifierGroups(entry.ModifierGroups, props, ctx);
        return props;
    }

    public static bool IsHidden(ProtocolSelectionEntry entry, EvalContext ctx) =>
        Apply(entry, ctx).Hidden;

    private static void ApplyModifiers(List<ProtocolModifier>? modifiers, ModifiedProperties props, EvalContext ctx)
    {
        if (modifiers is null) return;
        foreach (var mod in modifiers)
        {
            if (!EvaluateAllConditions(mod.Conditions, mod.ConditionGroups, ctx))
                continue;
            int repeatCount = mod.Repeats is { Count: > 0 }
                ? CalculateRepeatCount(mod.Repeats, ctx) : 1;
            for (int i = 0; i < repeatCount; i++)
                ApplySingleModifier(mod, props);
        }
    }

    private static void ApplyModifierGroups(List<ProtocolModifierGroup>? groups, ModifiedProperties props, EvalContext ctx)
    {
        if (groups is null) return;
        foreach (var group in groups)
        {
            if (!EvaluateAllConditions(group.Conditions, group.ConditionGroups, ctx))
                continue;
            int repeatCount = group.Repeats is { Count: > 0 }
                ? CalculateRepeatCount(group.Repeats, ctx) : 1;
            for (int i = 0; i < repeatCount; i++)
            {
                ApplyModifiers(group.Modifiers, props, ctx);
                ApplyModifierGroups(group.ModifierGroups, props, ctx);
            }
        }
    }

    private static void ApplySingleModifier(ProtocolModifier mod, ModifiedProperties props)
    {
        switch (mod.Field)
        {
            case "name":
                ApplyStringModifier(mod, s => props.Name = s, () => props.Name);
                break;
            case "hidden":
                if (mod.Type == "set")
                    props.Hidden = string.Equals(mod.Value, "true", StringComparison.OrdinalIgnoreCase);
                break;
            case "page":
                ApplyStringModifier(mod, s => props.Page = s, () => props.Page ?? "");
                break;
            default:
                // Cost type ID
                for (int i = 0; i < props.Costs.Count; i++)
                {
                    if (props.Costs[i].TypeId == mod.Field)
                    {
                        var cost = props.Costs[i];
                        if (double.TryParse(mod.Value, System.Globalization.NumberStyles.Any,
                            System.Globalization.CultureInfo.InvariantCulture, out var val))
                        {
                            props.Costs[i] = mod.Type switch
                            {
                                "set" => cost with { Value = val },
                                "increment" => cost with { Value = cost.Value + val },
                                "decrement" => cost with { Value = cost.Value - val },
                                _ => cost,
                            };
                        }
                        return;
                    }
                }
                // Constraint ID
                if (props.ConstraintValues.TryGetValue(mod.Field, out var existingVal) &&
                    double.TryParse(mod.Value, System.Globalization.NumberStyles.Any,
                        System.Globalization.CultureInfo.InvariantCulture, out var cval))
                {
                    props.ConstraintValues[mod.Field] = mod.Type switch
                    {
                        "set" => cval,
                        "increment" => existingVal + cval,
                        "decrement" => existingVal - cval,
                        _ => existingVal,
                    };
                }
                break;
        }
    }

    private static void ApplyStringModifier(ProtocolModifier mod, Action<string> setter, Func<string> getter)
    {
        switch (mod.Type)
        {
            case "set":
                setter(mod.Value);
                break;
            case "append":
                var current = getter();
                setter(string.IsNullOrEmpty(current) ? mod.Value : current + " " + mod.Value);
                break;
            case "increment":
                if (double.TryParse(getter(), System.Globalization.NumberStyles.Any,
                    System.Globalization.CultureInfo.InvariantCulture, out var incBase) &&
                    double.TryParse(mod.Value, System.Globalization.NumberStyles.Any,
                    System.Globalization.CultureInfo.InvariantCulture, out var incVal))
                    setter(FormatNumber(incBase + incVal));
                break;
            case "decrement":
                if (double.TryParse(getter(), System.Globalization.NumberStyles.Any,
                    System.Globalization.CultureInfo.InvariantCulture, out var decBase) &&
                    double.TryParse(mod.Value, System.Globalization.NumberStyles.Any,
                    System.Globalization.CultureInfo.InvariantCulture, out var decVal))
                    setter(FormatNumber(decBase - decVal));
                break;
        }
    }

    internal static string FormatNumber(double value) =>
        value == Math.Floor(value)
            ? ((long)value).ToString(System.Globalization.CultureInfo.InvariantCulture)
            : value.ToString(System.Globalization.CultureInfo.InvariantCulture);

    // ===== Condition evaluation =====

    internal static bool EvaluateAllConditions(
        List<ProtocolCondition>? conditions,
        List<ProtocolConditionGroup>? groups,
        EvalContext ctx)
    {
        if ((conditions is null or { Count: 0 }) && (groups is null or { Count: 0 }))
            return true;
        if (conditions is not null)
            foreach (var cond in conditions)
                if (!EvaluateCondition(cond, ctx))
                    return false;
        if (groups is not null)
            foreach (var group in groups)
                if (!EvaluateConditionGroup(group, ctx))
                    return false;
        return true;
    }

    private static bool EvaluateConditionGroup(ProtocolConditionGroup group, EvalContext ctx)
    {
        if (group.Type == "or")
        {
            bool any = false;
            if (group.Conditions is { Count: > 0 })
                foreach (var c in group.Conditions)
                    if (EvaluateCondition(c, ctx)) { any = true; break; }
            if (!any && group.ConditionGroups is { Count: > 0 })
                foreach (var cg in group.ConditionGroups)
                    if (EvaluateConditionGroup(cg, ctx)) { any = true; break; }
            return any || (group.Conditions is null or { Count: 0 }) && (group.ConditionGroups is null or { Count: 0 });
        }
        // AND (default)
        if (group.Conditions is not null)
            foreach (var c in group.Conditions)
                if (!EvaluateCondition(c, ctx))
                    return false;
        if (group.ConditionGroups is not null)
            foreach (var cg in group.ConditionGroups)
                if (!EvaluateConditionGroup(cg, ctx))
                    return false;
        return true;
    }

    private static bool EvaluateCondition(ProtocolCondition cond, EvalContext ctx)
    {
        // instanceOf/notInstanceOf
        if (cond.Type == "instanceOf" || cond.Type == "notInstanceOf")
        {
            if (cond.Scope == "self")
            {
                bool matched = CheckInstanceOf(cond.ChildId, ctx);
                return cond.Type == "instanceOf" ? matched : !matched;
            }
            // Non-self scopes: BattleScribe checks the scope element itself (force, roster),
            // not selections within it. These elements are never instances of selection entries,
            // so instanceOf always returns false (notInstanceOf always true).
            return cond.Type == "notInstanceOf";
        }

        // Scope=self for non-instanceOf: BattleScribe returns 0 (doesn't count the selection itself)
        if (cond.Scope == "self" && cond.Field == "selections")
        {
            return cond.Type switch
            {
                "atLeast" => 0 >= cond.Value,
                "atMost" => true,
                "greaterThan" => false,
                "lessThan" => 0 < cond.Value,
                "equalTo" => Math.Abs(cond.Value) < 1e-9,
                "notEqualTo" => Math.Abs(cond.Value) >= 1e-9,
                _ => true,
            };
        }

        // BattleScribe behavior: null/empty childId with non-self scope returns NaN → false
        if (string.IsNullOrEmpty(cond.ChildId) && cond.Scope != "self"
            && cond.Type != "instanceOf" && cond.Type != "notInstanceOf")
            return false;

        double count = CountInScope(cond.Field, cond.Scope, cond.ChildId, cond.IncludeChildSelections, ctx);

        // percentValue: threshold = totalSelectionsInScope * value / 100
        double threshold = cond.Value;
        if (cond.PercentValue)
        {
            var total = CountTotalSelectionsInScope(cond.Scope, cond.IncludeChildSelections, ctx);
            threshold = total * cond.Value / 100.0;
        }

        return cond.Type switch
        {
            "atLeast" => count >= threshold,
            "atMost" => count <= threshold,
            "greaterThan" => count > threshold,
            "lessThan" => count < threshold,
            "equalTo" => Math.Abs(count - threshold) < 1e-9,
            "notEqualTo" => Math.Abs(count - threshold) >= 1e-9,
            _ => true,
        };
    }

    private static bool CheckInstanceOf(string childId, EvalContext ctx)
    {
        if (ctx.Selection is null) return false;

        if (ctx.Selection.Entry.Id == childId) return true;
        if (ctx.Selection.Entry.Type == childId) return true;

        if (ctx.Selection.Entry.CategoryLinks is { Count: > 0 })
            foreach (var cl in ctx.Selection.Entry.CategoryLinks)
                if (cl.TargetId == childId) return true;

        return false;
    }

    internal static double CountInScope(string field, string scope, string childId, bool includeChildren, EvalContext ctx)
    {
        string targetId = string.IsNullOrEmpty(childId) ? ctx.OwnerEntryId : childId;

        if (field == "forces")
        {
            if (string.IsNullOrEmpty(childId))
                return ctx.AllForces.Count;
            return ctx.AllForces.Count(f => f.ForceEntry.Id == targetId);
        }

        var selections = GetSelectionsInScope(scope, includeChildren, ctx);

        if (field == "selections")
            return selections.Where(s => GetEffectiveId(s) == targetId).Sum(s => s.Number);

        // Cost type ID: sum cost values
        return selections
            .Where(s => string.IsNullOrEmpty(childId) || GetEffectiveId(s) == targetId)
            .Sum(s => GetSelectionCostValue(s, field));
    }

    private static IEnumerable<RosterSelection> GetSelectionsInScope(string scope, bool includeChildren, EvalContext ctx)
    {
        return scope switch
        {
            "self" => ctx.Selection != null ? [ctx.Selection] : [],
            "parent" => GetParentScope(includeChildren, ctx),
            "force" => GetForceScope(includeChildren, ctx),
            "roster" => GetRosterScope(includeChildren, ctx),
            "primary-catalogue" => GetForceScope(true, ctx),
            _ => GetRosterScope(true, ctx),
        };
    }

    private static IEnumerable<RosterSelection> GetParentScope(bool includeChildren, EvalContext ctx)
    {
        IEnumerable<RosterSelection> items;
        if (ctx.ParentSelection != null)
            items = ctx.ParentSelection.Children;
        else if (ctx.Force != null)
            items = ctx.Force.Selections;
        else
            return [];
        return includeChildren ? items.SelectMany(Flatten) : items;
    }

    private static IEnumerable<RosterSelection> GetForceScope(bool includeChildren, EvalContext ctx)
    {
        if (ctx.Force is null) return [];
        return includeChildren
            ? ctx.Force.Selections.SelectMany(Flatten)
            : ctx.Force.Selections;
    }

    private static IEnumerable<RosterSelection> GetRosterScope(bool includeChildren, EvalContext ctx)
    {
        return ctx.AllForces.SelectMany(f =>
            includeChildren ? f.Selections.SelectMany(Flatten) : f.Selections);
    }

    private static double CountTotalSelectionsInScope(string scope, bool includeChildren, EvalContext ctx)
    {
        var selections = GetSelectionsInScope(scope, includeChildren, ctx);
        return selections.Sum(s => (double)s.Number);
    }

    internal static IEnumerable<RosterSelection> Flatten(RosterSelection sel)
    {
        yield return sel;
        foreach (var child in sel.Children)
            foreach (var desc in Flatten(child))
                yield return desc;
    }

    internal static string GetEffectiveId(RosterSelection sel) =>
        sel.SourceLink?.Id ?? sel.Entry.Id;

    private static double GetSelectionCostValue(RosterSelection sel, string costTypeId)
    {
        if (sel.Entry.Costs is null) return 0;
        var cost = sel.Entry.Costs.FirstOrDefault(c => c.TypeId == costTypeId);
        if (cost is null) return 0;
        return sel.Entry.Collective ? cost.Value : cost.Value * sel.Number;
    }

    private static int CalculateRepeatCount(List<ProtocolRepeat>? repeats, EvalContext ctx)
    {
        if (repeats is null or { Count: 0 }) return 1;
        int total = 0;
        foreach (var repeat in repeats)
        {
            double count = CountInScope(repeat.Field, repeat.Scope, repeat.ChildId, repeat.IncludeChildSelections, ctx);
            if (repeat.Value > 0)
            {
                double raw = count / repeat.Value;
                int times = repeat.RoundUp ? (int)Math.Ceiling(raw) : (int)Math.Floor(raw);
                total += Math.Max(0, times) * repeat.Repeats;
            }
        }
        return total > 0 ? total : 0;
    }
}

internal sealed class ModifiedProperties
{
    public string Name { get; set; } = "";
    public bool Hidden { get; set; }
    public string? Page { get; set; }
    public List<CostValue> Costs { get; set; } = [];
    public Dictionary<string, double> ConstraintValues { get; set; } = new();
}

internal sealed class ModifiedProfileProperties
{
    public string Name { get; set; } = "";
    public bool Hidden { get; set; }
    public Dictionary<string, string> CharacteristicValues { get; set; } = new();
}

internal sealed class ModifiedRuleProperties
{
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public bool Hidden { get; set; }
}

internal record struct CostValue(string Name, string TypeId, double Value);

internal sealed class EvalContext
{
    public required List<RosterForce> AllForces { get; init; }
    public required RosterForce Force { get; init; }
    public RosterSelection? Selection { get; init; }
    public RosterSelection? ParentSelection { get; init; }
    public required string OwnerEntryId { get; init; }
}
