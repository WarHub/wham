using BattleScribeSpec;
using BattleScribeSpec.Protocol;

namespace WarHub.ArmouryModel.RosterEngine;

/// <summary>
/// Validates constraints and generates validation errors.
/// </summary>
internal sealed class ConstraintValidator
{
    private readonly ProtocolGameSystem _gameSystem;
    private readonly List<RosterForce> _forces;
    private readonly ModifierEvaluator _evaluator;
    private readonly Dictionary<string, double> _costLimits;
    private readonly EntryResolver _resolver;

    public ConstraintValidator(
        ProtocolGameSystem gameSystem,
        List<RosterForce> forces,
        ModifierEvaluator evaluator,
        Dictionary<string, double> costLimits,
        EntryResolver resolver)
    {
        _gameSystem = gameSystem;
        _forces = forces;
        _evaluator = evaluator;
        _costLimits = costLimits;
        _resolver = resolver;
    }

    public List<ValidationErrorState> Validate()
    {
        var errors = new List<ValidationErrorState>();

        foreach (var force in _forces)
        {
            ValidateForce(force, errors);
        }

        ValidateCostLimits(errors);

        return errors;
    }

    private void ValidateForce(RosterForce force, List<ValidationErrorState> errors)
    {
        // Get available entries and validate constraints on each
        var available = _resolver.GetAvailableEntries(force.Catalogue);

        // Track shared constraints by ID
        var sharedConstraints = new Dictionary<string, (ProtocolConstraint Constraint, ProtocolSelectionEntry Entry, double Count)>(StringComparer.Ordinal);

        foreach (var avail in available)
        {
            if (avail.Entry is { } entry)
            {
                ValidateEntryConstraints(entry, force, errors, sharedConstraints);
            }
        }

        // Validate hidden entries that have selections
        foreach (var sel in force.Selections)
        {
            var effectiveHidden = _evaluator.GetEffectiveHidden(sel.Entry, sel, force);
            if (effectiveHidden)
            {
                // Find the primary category for error reporting
                var primaryCatId = ModifierEvaluator.GetPrimaryCategory(sel.Entry);
                errors.Add(new ValidationErrorState(
                    Message: $"{force.ForceEntry.Name} cannot have any selections of {sel.Entry.Name} (hidden)",
                    OwnerType: primaryCatId is not null ? "category" : "force",
                    OwnerEntryId: primaryCatId,
                    EntryId: sel.Entry.Id,
                    ConstraintId: "hidden"
                ));
            }

            // Validate child selection constraints
            ValidateChildSelectionConstraints(sel, force, errors);
        }
    }

    private void ValidateEntryConstraints(ProtocolSelectionEntry entry, RosterForce force,
        List<ValidationErrorState> errors,
        Dictionary<string, (ProtocolConstraint Constraint, ProtocolSelectionEntry Entry, double Count)> sharedConstraints)
    {
        if (entry.Constraints is not { } constraints) return;

        foreach (var constraint in constraints)
        {
            if (constraint.Scope != "parent") continue;

            var effectiveValue = _evaluator.GetEffectiveConstraintValue(constraint, entry, null, force);
            var count = CountForConstraint(constraint, entry, force);

            if (constraint.Shared)
            {
                if (sharedConstraints.TryGetValue(constraint.Id, out var existing))
                {
                    // Update the count
                    sharedConstraints[constraint.Id] = (existing.Constraint, existing.Entry, existing.Count + count);
                    continue; // Will be validated after collecting all
                }
                sharedConstraints[constraint.Id] = (constraint, entry, count);
                continue;
            }

            CheckConstraint(constraint, effectiveValue, count, entry, force, errors);
        }

        // Validate shared constraints
        foreach (var (id, (constraint, sharedEntry, totalCount)) in sharedConstraints)
        {
            var effectiveValue = _evaluator.GetEffectiveConstraintValue(constraint, sharedEntry, null, force);
            CheckConstraint(constraint, effectiveValue, totalCount, sharedEntry, force, errors);
        }
    }

    private double CountForConstraint(ProtocolConstraint constraint, ProtocolSelectionEntry entry, RosterForce force)
    {
        if (constraint.Field == "selections")
        {
            double count = 0;
            foreach (var sel in force.Selections)
            {
                if (sel.Entry.Id == entry.Id)
                    count += sel.Number;
                if (constraint.IncludeChildSelections)
                {
                    count += CountChildSelections(sel.Children, entry.Id);
                }
            }
            return count;
        }

        if (constraint.Field == "forces")
        {
            return _forces.Count;
        }

        // Cost type field
        double costTotal = 0;
        foreach (var sel in force.Selections)
        {
            if (sel.Entry.Id == entry.Id)
            {
                var costs = _evaluator.GetEffectiveCosts(sel.Entry, sel, force);
                var cost = costs.FirstOrDefault(c => c.TypeId == constraint.Field);
                if (cost is not null)
                    costTotal += cost.Value * (sel.Entry.Collective ? 1 : sel.Number);
            }
        }
        return costTotal;
    }

    private static double CountChildSelections(List<RosterSelection> children, string entryId)
    {
        double count = 0;
        foreach (var child in children)
        {
            if (child.Entry.Id == entryId)
                count += child.Number;
            count += CountChildSelections(child.Children, entryId);
        }
        return count;
    }

    private void CheckConstraint(ProtocolConstraint constraint, double effectiveValue, double count,
        ProtocolSelectionEntry entry, RosterForce force, List<ValidationErrorState> errors)
    {
        var primaryCatId = ModifierEvaluator.GetPrimaryCategory(entry);

        if (constraint.Type == "min" && count < effectiveValue)
        {
            var needed = effectiveValue - count;
            errors.Add(new ValidationErrorState(
                Message: $"{force.ForceEntry.Name} must have {needed} more selections of {entry.Name} (minimum {effectiveValue})",
                OwnerType: primaryCatId is not null ? "category" : "force",
                OwnerEntryId: primaryCatId,
                EntryId: entry.Id,
                ConstraintId: constraint.Id
            ));
        }
        else if (constraint.Type == "max" && count > effectiveValue)
        {
            var over = count - effectiveValue;
            errors.Add(new ValidationErrorState(
                Message: $"{force.ForceEntry.Name} has too many selections of {entry.Name} (maximum {effectiveValue})",
                OwnerType: primaryCatId is not null ? "category" : "force",
                OwnerEntryId: primaryCatId,
                EntryId: entry.Id,
                ConstraintId: constraint.Id
            ));
        }
    }

    private void ValidateChildSelectionConstraints(RosterSelection selection, RosterForce force,
        List<ValidationErrorState> errors)
    {
        foreach (var child in selection.Children)
        {
            if (child.Entry.Constraints is { } constraints)
            {
                foreach (var constraint in constraints)
                {
                    if (constraint.Scope != "parent") continue;

                    var effectiveValue = _evaluator.GetEffectiveConstraintValue(constraint, child.Entry, child, force);
                    double count = 0;
                    foreach (var sibling in selection.Children)
                    {
                        if (sibling.Entry.Id == child.Entry.Id)
                            count += sibling.Number;
                    }

                    CheckConstraint(constraint, effectiveValue, count, child.Entry, force, errors);
                }
            }

            ValidateChildSelectionConstraints(child, force, errors);
        }
    }

    private void ValidateCostLimits(List<ValidationErrorState> errors)
    {
        // Aggregate all costs
        var totalCosts = new Dictionary<string, double>(StringComparer.Ordinal);

        foreach (var force in _forces)
        {
            foreach (var sel in force.Selections)
            {
                AggregateCosts(sel, totalCosts, force);
            }
        }

        // Check limits
        foreach (var (typeId, limit) in _costLimits)
        {
            if (limit < 0) continue; // -1 = unlimited
            totalCosts.TryGetValue(typeId, out var actual);
            if (actual > limit)
            {
                var over = actual - limit;
                var typeName = _gameSystem.CostTypes?.FirstOrDefault(ct => ct.Id == typeId)?.Name ?? typeId;
                errors.Add(new ValidationErrorState(
                    Message: $"Roster is over the {typeName} limit by {over}{typeName}",
                    OwnerType: "roster",
                    EntryId: "costLimits",
                    ConstraintId: typeId
                ));
            }
        }
    }

    private void AggregateCosts(RosterSelection sel, Dictionary<string, double> totals, RosterForce force)
    {
        var costs = _evaluator.GetEffectiveCosts(sel.Entry, sel, force);
        foreach (var cost in costs)
        {
            totals.TryGetValue(cost.TypeId, out var current);
            totals[cost.TypeId] = current + cost.Value * (sel.Entry.Collective ? 1 : sel.Number);
        }

        foreach (var child in sel.Children)
        {
            AggregateCosts(child, totals, force);
        }
    }
}
