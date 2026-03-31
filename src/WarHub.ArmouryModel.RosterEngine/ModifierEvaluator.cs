using BattleScribeSpec.Protocol;

namespace WarHub.ArmouryModel.RosterEngine;

/// <summary>
/// Evaluates modifiers and conditions against current roster state.
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

    /// <summary>
    /// Get the effective name of an entry after applying modifiers.
    /// </summary>
    public string GetEffectiveName(ProtocolSelectionEntry entry, RosterSelection? selection, RosterForce? force)
    {
        var name = entry.Name;
        if (entry.Modifiers is not { Count: > 0 } && entry.ModifierGroups is not { Count: > 0 })
            return name;

        name = ApplyModifiersToField(entry.Modifiers, entry.ModifierGroups, "name", name, entry, selection, force);
        return name;
    }

    /// <summary>
    /// Get the effective hidden state after applying modifiers.
    /// </summary>
    public bool GetEffectiveHidden(ProtocolSelectionEntry entry, RosterSelection? selection, RosterForce? force)
    {
        var hidden = entry.Hidden;
        var result = ApplyModifiersToField(entry.Modifiers, entry.ModifierGroups, "hidden",
            hidden.ToString().ToLowerInvariant(), entry, selection, force);
        return string.Equals(result, "true", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Get effective costs after applying modifiers.
    /// </summary>
    public List<ProtocolCostValue> GetEffectiveCosts(ProtocolSelectionEntry entry, RosterSelection? selection, RosterForce? force)
    {
        var costs = new Dictionary<string, double>(StringComparer.Ordinal);

        // Start with base costs
        if (entry.Costs is { } baseCosts)
        {
            foreach (var cost in baseCosts)
                costs[cost.TypeId] = cost.Value;
        }

        // Apply modifiers that target cost type IDs
        ApplyCostModifiers(entry.Modifiers, entry.ModifierGroups, costs, entry, selection, force);

        return costs.Select(kvp => new ProtocolCostValue
        {
            TypeId = kvp.Key,
            Name = GetCostTypeName(kvp.Key),
            Value = kvp.Value
        }).ToList();
    }

    /// <summary>
    /// Get the effective value of a constraint after applying modifiers.
    /// </summary>
    public double GetEffectiveConstraintValue(ProtocolConstraint constraint, ProtocolSelectionEntry entry,
        RosterSelection? selection, RosterForce? force)
    {
        var value = constraint.Value;

        // Modifiers can target constraint fields by constraint ID
        if (entry.Modifiers is { } modifiers)
        {
            foreach (var mod in modifiers)
            {
                if (mod.Field == constraint.Id && EvaluateConditions(mod, entry, selection, force))
                {
                    var repeatCount = GetRepeatCount(mod.Repeats, entry, selection, force);
                    value = ApplyModifierValue(mod.Type, value, mod.Value, repeatCount);
                }
            }
        }

        if (entry.ModifierGroups is { } groups)
        {
            foreach (var group in groups)
            {
                if (!EvaluateGroupConditions(group, entry, selection, force)) continue;
                var groupRepeatCount = GetRepeatCount(group.Repeats, entry, selection, force);
                if (group.Modifiers is { } groupMods)
                {
                    foreach (var mod in groupMods)
                    {
                        if (mod.Field == constraint.Id && EvaluateConditions(mod, entry, selection, force))
                        {
                            var repeatCount = GetRepeatCount(mod.Repeats, entry, selection, force) * groupRepeatCount;
                            if (repeatCount == 0) repeatCount = 1;
                            value = ApplyModifierValue(mod.Type, value, mod.Value, repeatCount);
                        }
                    }
                }
            }
        }

        return value;
    }

    private string ApplyModifiersToField(List<ProtocolModifier>? modifiers, List<ProtocolModifierGroup>? groups,
        string targetField, string currentValue, ProtocolSelectionEntry entry, RosterSelection? selection, RosterForce? force)
    {
        if (modifiers is { } mods)
        {
            foreach (var mod in mods)
            {
                if (mod.Field != targetField) continue;
                if (!EvaluateConditions(mod, entry, selection, force)) continue;
                var repeatCount = GetRepeatCount(mod.Repeats, entry, selection, force);
                currentValue = ApplyStringModifier(mod.Type, currentValue, mod.Value, repeatCount);
            }
        }

        if (groups is { } grps)
        {
            foreach (var group in grps)
            {
                if (!EvaluateGroupConditions(group, entry, selection, force)) continue;
                var groupRepeatCount = GetRepeatCount(group.Repeats, entry, selection, force);
                currentValue = ApplyModifiersToField(group.Modifiers, group.ModifierGroups, targetField,
                    currentValue, entry, selection, force);
            }
        }

        return currentValue;
    }

    private void ApplyCostModifiers(List<ProtocolModifier>? modifiers, List<ProtocolModifierGroup>? groups,
        Dictionary<string, double> costs, ProtocolSelectionEntry entry, RosterSelection? selection, RosterForce? force)
    {
        if (modifiers is { } mods)
        {
            foreach (var mod in mods)
            {
                // Cost modifier: field is a cost type ID
                if (!IsCostField(mod.Field)) continue;
                if (!EvaluateConditions(mod, entry, selection, force)) continue;
                var repeatCount = GetRepeatCount(mod.Repeats, entry, selection, force);
                costs.TryGetValue(mod.Field, out var current);
                costs[mod.Field] = ApplyModifierValue(mod.Type, current, mod.Value, repeatCount);
            }
        }

        if (groups is { } grps)
        {
            foreach (var group in grps)
            {
                if (!EvaluateGroupConditions(group, entry, selection, force)) continue;
                ApplyCostModifiers(group.Modifiers, group.ModifierGroups, costs, entry, selection, force);
            }
        }
    }

    private bool IsCostField(string field)
    {
        return field != "name" && field != "hidden" && field != "description" && field != "page"
            && field != "publicationId" && field != "category"
            && _gameSystem.CostTypes?.Any(ct => ct.Id == field) == true;
    }

    public bool EvaluateConditions(ProtocolModifier mod, ProtocolSelectionEntry entry,
        RosterSelection? selection, RosterForce? force)
    {
        // All conditions must be true (AND)
        if (mod.Conditions is { Count: > 0 } conditions)
        {
            foreach (var cond in conditions)
            {
                if (!EvaluateCondition(cond, entry, selection, force))
                    return false;
            }
        }

        // Condition groups
        if (mod.ConditionGroups is { Count: > 0 } condGroups)
        {
            foreach (var group in condGroups)
            {
                if (!EvaluateConditionGroup(group, entry, selection, force))
                    return false;
            }
        }

        return true;
    }

    private bool EvaluateGroupConditions(ProtocolModifierGroup group, ProtocolSelectionEntry entry,
        RosterSelection? selection, RosterForce? force)
    {
        if (group.Conditions is { Count: > 0 } conditions)
        {
            foreach (var cond in conditions)
            {
                if (!EvaluateCondition(cond, entry, selection, force))
                    return false;
            }
        }

        if (group.ConditionGroups is { Count: > 0 } condGroups)
        {
            foreach (var cg in condGroups)
            {
                if (!EvaluateConditionGroup(cg, entry, selection, force))
                    return false;
            }
        }

        return true;
    }

    internal bool EvaluateConditionGroup(ProtocolConditionGroup group, ProtocolSelectionEntry entry,
        RosterSelection? selection, RosterForce? force)
    {
        var isOr = string.Equals(group.Type, "or", StringComparison.OrdinalIgnoreCase);

        if (group.Conditions is { } conditions)
        {
            foreach (var cond in conditions)
            {
                var result = EvaluateCondition(cond, entry, selection, force);
                if (isOr && result) return true;
                if (!isOr && !result) return false;
            }
        }

        if (group.ConditionGroups is { } subGroups)
        {
            foreach (var subGroup in subGroups)
            {
                var result = EvaluateConditionGroup(subGroup, entry, selection, force);
                if (isOr && result) return true;
                if (!isOr && !result) return false;
            }
        }

        // AND with no failing = true, OR with no passing = false
        return !isOr;
    }

    internal bool EvaluateCondition(ProtocolCondition condition, ProtocolSelectionEntry entry,
        RosterSelection? selection, RosterForce? force)
    {
        var count = CountInScope(condition, entry, selection, force);
        var value = condition.Value;

        return condition.Type switch
        {
            "atLeast" => count >= value,
            "atMost" => count <= value,
            "greaterThan" => count > value,
            "lessThan" => count < value,
            "equalTo" => Math.Abs(count - value) < 0.0001,
            "notEqualTo" => Math.Abs(count - value) >= 0.0001,
            "instanceOf" => count >= 1,
            "notInstanceOf" => count < 1,
            _ => true
        };
    }

    internal double CountInScope(ProtocolCondition condition, ProtocolSelectionEntry entry,
        RosterSelection? selection, RosterForce? force)
    {
        var scope = condition.Scope;
        var field = condition.Field;
        var childId = condition.ChildId;
        var includeChildSelections = condition.IncludeChildSelections;

        return scope switch
        {
            "self" => CountSelfScope(field, childId, entry, selection, includeChildSelections),
            "parent" => CountParentScope(field, childId, selection, force, includeChildSelections),
            "force" => CountForceScope(field, childId, force, includeChildSelections),
            "roster" => CountRosterScope(field, childId, includeChildSelections),
            "primary-catalogue" => CountRosterScope(field, childId, includeChildSelections),
            "primary-category" => CountPrimaryCategoryScope(field, childId, entry, selection, force, includeChildSelections),
            "ancestor" => CountParentScope(field, childId, selection, force, includeChildSelections),
            _ => 0
        };
    }

    private double CountSelfScope(string field, string? childId, ProtocolSelectionEntry entry,
        RosterSelection? selection, bool includeChildren)
    {
        if (field == "selections")
        {
            if (selection is null) return 0;
            // Count among siblings that match childId
            return CountSelectionsMatching(selection.Children, childId, includeChildren);
        }
        if (field == "forces") return 0;
        // Cost type field
        return SumCostsInSelections(selection is null ? [] : [selection], field, childId, includeChildren);
    }

    private double CountParentScope(string field, string? childId, RosterSelection? selection,
        RosterForce? force, bool includeChildren)
    {
        if (field == "selections")
        {
            if (selection?.Children is { } children && children.Count > 0)
            {
                // If this is a parent with children, count children
                // But if we're evaluating from a child, count siblings
            }

            if (force is not null)
            {
                return CountSelectionsMatching(force.Selections, childId, includeChildren);
            }
            return 0;
        }
        if (field == "forces") return _forces.Count;
        return SumCostsInSelections(force?.Selections ?? [], field, childId, includeChildren);
    }

    private double CountForceScope(string field, string? childId, RosterForce? force, bool includeChildren)
    {
        if (force is null) return 0;
        if (field == "selections")
            return CountSelectionsMatching(force.Selections, childId, includeChildren || true);
        if (field == "forces") return 1;
        return SumCostsInSelections(force.Selections, field, childId, true);
    }

    private double CountRosterScope(string field, string? childId, bool includeChildren)
    {
        if (field == "forces") return _forces.Count;
        if (field == "selections")
        {
            double total = 0;
            foreach (var f in _forces)
                total += CountSelectionsMatching(f.Selections, childId, true);
            return total;
        }
        // Cost field
        double costTotal = 0;
        foreach (var f in _forces)
            costTotal += SumCostsInSelections(f.Selections, field, childId, true);
        return costTotal;
    }

    private double CountPrimaryCategoryScope(string field, string? childId, ProtocolSelectionEntry entry,
        RosterSelection? selection, RosterForce? force, bool includeChildren)
    {
        // Find the primary category of this entry
        var primaryCatId = GetPrimaryCategory(entry);
        if (primaryCatId is null) return 0;

        // Count all selections in the force that share this primary category
        if (force is null) return 0;
        if (field == "selections")
        {
            double count = 0;
            foreach (var sel in force.Selections)
            {
                if (HasPrimaryCategory(sel.Entry, primaryCatId))
                {
                    if (childId is null || sel.Entry.Id == childId)
                        count += sel.Number;
                    if (includeChildren)
                        count += CountSelectionsMatching(sel.Children, childId, true);
                }
            }
            return count;
        }
        return 0;
    }

    private static double CountSelectionsMatching(List<RosterSelection> selections, string? childId, bool includeChildren)
    {
        double count = 0;
        foreach (var sel in selections)
        {
            if (childId is null || sel.Entry.Id == childId)
                count += sel.Number;
            if (includeChildren)
                count += CountSelectionsMatching(sel.Children, childId, includeChildren);
        }
        return count;
    }

    private double SumCostsInSelections(List<RosterSelection> selections, string costTypeId, string? childId, bool includeChildren)
    {
        double total = 0;
        foreach (var sel in selections)
        {
            if (childId is null || sel.Entry.Id == childId)
            {
                var costs = GetEffectiveCosts(sel.Entry, sel, null);
                var cost = costs.FirstOrDefault(c => c.TypeId == costTypeId);
                if (cost is not null)
                {
                    total += cost.Value * (sel.Entry.Collective ? 1 : sel.Number);
                }
            }
            if (includeChildren)
            {
                total += SumCostsInSelections(sel.Children, costTypeId, childId, true);
            }
        }
        return total;
    }

    internal int GetRepeatCount(List<ProtocolRepeat>? repeats, ProtocolSelectionEntry entry,
        RosterSelection? selection, RosterForce? force)
    {
        if (repeats is not { Count: > 0 }) return 1;

        var totalRepeats = 0;
        foreach (var repeat in repeats)
        {
            var condition = new ProtocolCondition
            {
                Field = repeat.Field,
                Scope = repeat.Scope,
                ChildId = repeat.ChildId,
                IncludeChildSelections = repeat.IncludeChildSelections,
                IncludeChildForces = repeat.IncludeChildForces,
                Shared = repeat.Shared,
                PercentValue = repeat.PercentValue
            };

            var count = CountInScope(condition, entry, selection, force);
            var repeatsFromThis = count * repeat.Repeats;

            if (repeat.RoundUp)
                totalRepeats += (int)Math.Ceiling(repeatsFromThis);
            else
                totalRepeats += (int)Math.Floor(repeatsFromThis);
        }

        return Math.Max(totalRepeats, 0);
    }

    private static double ApplyModifierValue(string type, double current, string valueStr, int repeatCount)
    {
        if (!double.TryParse(valueStr, System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture, out var value))
            return current;

        if (repeatCount <= 0) repeatCount = 1;

        return type switch
        {
            "set" => value,
            "increment" => current + (value * repeatCount),
            "decrement" => current - (value * repeatCount),
            _ => current
        };
    }

    private static string ApplyStringModifier(string type, string current, string value, int repeatCount)
    {
        if (repeatCount <= 0) repeatCount = 1;

        return type switch
        {
            "set" => value,
            "append" => current + " " + value,
            _ => current
        };
    }

    private string GetCostTypeName(string typeId)
    {
        return _gameSystem.CostTypes?.FirstOrDefault(ct => ct.Id == typeId)?.Name ?? typeId;
    }

    internal static string? GetPrimaryCategory(ProtocolSelectionEntry entry)
    {
        if (entry.CategoryLinks is not { } links) return null;
        return links.FirstOrDefault(cl => cl.Primary)?.TargetId;
    }

    internal static bool HasPrimaryCategory(ProtocolSelectionEntry entry, string categoryId)
    {
        if (entry.CategoryLinks is not { } links) return false;
        return links.Any(cl => cl.Primary && cl.TargetId == categoryId);
    }
}
