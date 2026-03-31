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
            ValidateForceSelections(force, errors);
        }

        // Cost limit validation
        var totalCosts = AggregateTotalCosts();
        foreach (var (typeId, limit) in _costLimits)
        {
            if (limit < 0) continue;
            var actual = totalCosts.GetValueOrDefault(typeId, 0);
            if (actual > limit + 1e-9)
            {
                var costName = _gameSystem.CostTypes?.FirstOrDefault(ct => ct.Id == typeId)?.Name ?? typeId;
                errors.Add(new ValidationErrorState(
                    Message: $"Cost {costName} ({actual}) exceeds limit ({limit})",
                    OwnerType: "roster",
                    EntryId: "costLimits",
                    ConstraintId: typeId));
            }
        }

        return errors;
    }

    private Dictionary<string, double> AggregateTotalCosts()
    {
        var totals = new Dictionary<string, double>(StringComparer.Ordinal);
        if (_gameSystem.CostTypes is { } costTypes)
            foreach (var ct in costTypes)
                totals[ct.Id] = 0;

        foreach (var force in _forces)
            foreach (var sel in force.Selections)
                AggregateCostsRecursive(sel, totals, force);

        return totals;
    }

    private void AggregateCostsRecursive(RosterSelection sel, Dictionary<string, double> totals, RosterForce force)
    {
        var costs = _evaluator.GetEffectiveCosts(sel.Entry, sel, force);
        foreach (var cost in costs)
        {
            totals.TryGetValue(cost.TypeId, out var current);
            totals[cost.TypeId] = current + cost.Value * (sel.Entry.Collective ? 1 : sel.Number);
        }
        foreach (var child in sel.Children)
            AggregateCostsRecursive(child, totals, force);
    }

    private void ValidateForceSelections(RosterForce force, List<ValidationErrorState> errors)
    {
        // Build a map of entryId -> total count in this force
        var forceCounts = new Dictionary<string, int>();
        foreach (var sel in force.Selections)
        {
            var id = ModifierEvaluator.GetEffectiveId(sel);
            forceCounts[id] = forceCounts.GetValueOrDefault(id) + sel.Number;
        }

        // Check constraints on all available entries (not just selected ones)
        var available = _resolver.GetAvailableEntries(force.Catalogue);
        var sharedChecked = new HashSet<string>();

        foreach (var avail in available)
        {
            if (avail.IsGroup) continue;
            var entry = avail.Entry!;
            var effectiveId = avail.SourceLink?.Id ?? entry.Id;

            if (entry.Constraints is null) continue;

            var ctx = new EvalContext
            {
                AllForces = _forces,
                Force = force,
                Selection = null,
                ParentSelection = null,
                OwnerEntryId = effectiveId,
            };
            var modified = ModifierEvaluator.Apply(entry, ctx);

            foreach (var constraint in entry.Constraints)
            {
                if (constraint.Field != "selections" && constraint.Field != "forces") continue;

                // Handle shared constraints: only validate once per constraint ID
                if (constraint.Shared && !sharedChecked.Add(constraint.Id))
                    continue;

                var constraintValue = modified.ConstraintValues.GetValueOrDefault(constraint.Id, constraint.Value);

                if (constraint.Field == "forces")
                {
                    double forceCount = constraint.Scope == "roster"
                        ? _forces.Count
                        : 1;
                    string fOwner = constraint.Scope == "roster" ? "roster" : "force";
                    string? fOwnerId = constraint.Scope == "roster" ? null : null;
                    ValidateConstraint(constraint, constraintValue, forceCount, effectiveId,
                        fOwner, fOwnerId, errors, modified.Hidden);
                    continue;
                }

                var count = GetCountInScope(constraint, effectiveId, force);

                var (ot, oeid) = GetOwnerForEntry(entry, force);
                ValidateConstraint(constraint, constraintValue, count, effectiveId,
                    ot, oeid, errors, modified.Hidden);
            }
        }

        // Validate child selection constraints
        foreach (var sel in force.Selections)
        {
            ValidateChildConstraints(sel, force, errors);
        }

        // Validate category constraints
        ValidateCategoryConstraints(force, errors);
    }

    private void ValidateChildConstraints(
        RosterSelection parent,
        RosterForce force,
        List<ValidationErrorState> errors)
    {
        if (parent.Entry.SelectionEntries is null && parent.Entry.EntryLinks is null
            && parent.Entry.SelectionEntryGroups is null)
            return;

        var childCounts = new Dictionary<string, int>();
        foreach (var child in parent.Children)
        {
            var id = ModifierEvaluator.GetEffectiveId(child);
            childCounts[id] = childCounts.GetValueOrDefault(id) + child.Number;
        }

        var childEntries = _resolver.GetChildEntries(parent.Entry);
        foreach (var avail in childEntries)
        {
            if (avail.IsGroup) continue;
            var entry = avail.Entry!;
            var effectiveId = avail.SourceLink?.Id ?? entry.Id;

            if (entry.Constraints is null) continue;

            var ctx = new EvalContext
            {
                AllForces = _forces,
                Force = force,
                Selection = parent,
                ParentSelection = null,
                OwnerEntryId = effectiveId,
            };
            var modified = ModifierEvaluator.Apply(entry, ctx);

            foreach (var constraint in entry.Constraints)
            {
                if (constraint.Field != "selections") continue;
                if (constraint.Scope != "parent") continue;

                var constraintValue = modified.ConstraintValues.GetValueOrDefault(constraint.Id, constraint.Value);
                var count = childCounts.GetValueOrDefault(effectiveId);

                ValidateConstraint(constraint, constraintValue, count, effectiveId,
                    "selection", ModifierEvaluator.GetEffectiveId(parent), errors, modified.Hidden);
            }
        }

        // Recurse into children
        foreach (var child in parent.Children)
            ValidateChildConstraints(child, force, errors);
    }

    private void ValidateCategoryConstraints(RosterForce force, List<ValidationErrorState> errors)
    {
        // Count selections per category in this force
        var categoryCounts = new Dictionary<string, int>();
        foreach (var sel in force.Selections.SelectMany(ModifierEvaluator.Flatten))
        {
            if (sel.Entry.CategoryLinks is null) continue;
            foreach (var catLink in sel.Entry.CategoryLinks)
            {
                categoryCounts[catLink.TargetId] =
                    categoryCounts.GetValueOrDefault(catLink.TargetId) + sel.Number;
            }
        }

        // Check force entry category links for constraints
        if (force.ForceEntry.CategoryLinks is null) return;
        foreach (var catLink in force.ForceEntry.CategoryLinks)
        {
            if (catLink.Constraints is null) continue;
            var count = categoryCounts.GetValueOrDefault(catLink.TargetId);

            foreach (var constraint in catLink.Constraints)
            {
                if (constraint.Field != "selections") continue;
                var constraintValue = constraint.Value;

                if (constraint.Type == "min" && count < constraintValue - 1e-9 && constraintValue > 0)
                {
                    errors.Add(new ValidationErrorState(
                        Message: $"Min {constraintValue} required for category {catLink.Name}, have {count}",
                        OwnerType: "category",
                        OwnerEntryId: catLink.TargetId,
                        EntryId: catLink.TargetId,
                        ConstraintId: constraint.Id));
                }
                else if (constraint.Type == "max" && count > constraintValue + 1e-9)
                {
                    errors.Add(new ValidationErrorState(
                        Message: $"Max {constraintValue} allowed for category {catLink.Name}, have {count}",
                        OwnerType: "category",
                        OwnerEntryId: catLink.TargetId,
                        EntryId: catLink.TargetId,
                        ConstraintId: constraint.Id));
                }
            }
        }
    }

    private static (string ownerType, string? ownerEntryId) GetOwnerForEntry(
        ProtocolSelectionEntry entry, RosterForce force)
    {
        if (entry.CategoryLinks is { Count: > 0 })
        {
            var primary = entry.CategoryLinks.FirstOrDefault(cl => cl.Primary);
            if (primary != null)
                return ("category", primary.TargetId);
            return ("category", entry.CategoryLinks[0].TargetId);
        }
        return ("selection", entry.Id);
    }

    private double GetCountInScope(
        ProtocolConstraint constraint,
        string effectiveId,
        RosterForce force)
    {
        string targetId = effectiveId;

        IEnumerable<RosterSelection> selections = constraint.Scope switch
        {
            "parent" or "force" => constraint.IncludeChildSelections
                ? force.Selections.SelectMany(ModifierEvaluator.Flatten)
                : force.Selections,
            "roster" => _forces.SelectMany(f =>
                constraint.IncludeChildSelections
                    ? f.Selections.SelectMany(ModifierEvaluator.Flatten)
                    : f.Selections),
            _ => force.Selections,
        };

        return selections.Where(s => ModifierEvaluator.GetEffectiveId(s) == targetId).Sum(s => s.Number);
    }

    private static void ValidateConstraint(
        ProtocolConstraint constraint,
        double constraintValue,
        double count,
        string entryId,
        string ownerType,
        string? ownerEntryId,
        List<ValidationErrorState> errors,
        bool isHidden)
    {
        // Hidden entries that are selected generate a hidden error
        if (isHidden && count > 0)
        {
            errors.Add(new ValidationErrorState(
                Message: $"Entry {entryId} is hidden but has {count} selection(s)",
                OwnerType: ownerType,
                OwnerEntryId: ownerEntryId,
                EntryId: entryId,
                ConstraintId: "hidden"));
        }

        if (constraint.Type == "min" && count < constraintValue - 1e-9)
        {
            errors.Add(new ValidationErrorState(
                Message: $"Min {constraintValue} required for {entryId}, have {count}",
                OwnerType: ownerType,
                OwnerEntryId: ownerEntryId,
                EntryId: entryId,
                ConstraintId: constraint.Id));
        }
        else if (constraint.Type == "max" && constraintValue >= 0 && count > constraintValue + 1e-9)
        {
            errors.Add(new ValidationErrorState(
                Message: $"Max {constraintValue} allowed for {entryId}, have {count}",
                OwnerType: ownerType,
                OwnerEntryId: ownerEntryId,
                EntryId: entryId,
                ConstraintId: constraint.Id));
        }
    }
}
