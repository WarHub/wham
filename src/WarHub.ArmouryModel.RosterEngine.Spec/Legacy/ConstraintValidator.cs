using BattleScribeSpec;
using BattleScribeSpec.Protocol;

namespace WarHub.ArmouryModel.RosterEngine.Spec.Legacy;

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

        ValidateForceEntryConstraints(errors);
        ValidateCostLimits(errors);

        return errors;
    }

    private void ValidateCostLimits(List<ValidationErrorState> errors)
    {
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
    }

    private void ValidateForceEntryConstraints(List<ValidationErrorState> errors)
    {
        // Count forces per force entry type
        var forceCounts = new Dictionary<string, int>(StringComparer.Ordinal);
        foreach (var force in _forces)
        {
            forceCounts[force.ForceEntry.Id] = forceCounts.GetValueOrDefault(force.ForceEntry.Id) + 1;
        }

        // Each force validates its own ForceEntry's constraints
        foreach (var force in _forces)
        {
            if (force.ForceEntry.Constraints is not { } constraints) continue;
            foreach (var constraint in constraints)
            {
                if (constraint.Field != "forces") continue;
                var count = constraint.Scope == "roster"
                    ? forceCounts.GetValueOrDefault(force.ForceEntry.Id)
                    : 1;
                CheckConstraint(constraint, constraint.Value, count, force.ForceEntry.Id,
                    "roster", null, errors, false);
            }
        }
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
        var available = _resolver.GetAvailableEntries(force.Catalogue);
        var sharedChecked = new HashSet<(string constraintId, string entryId)>();

        foreach (var avail in available)
        {
            if (avail.IsGroup) continue;
            var entry = avail.Entry!;
            // Use entry.Id (target ID for entry links) for counting and errors
            var entryId = entry.Id;

            if (entry.Constraints is null && !entry.Hidden) continue;

            bool hasCategoryLinks = entry.CategoryLinks is { Count: > 0 };

            var ctx = new EvalContext
            {
                AllForces = _forces,
                Force = force,
                Selection = null,
                ParentSelection = null,
                OwnerEntryId = entryId,
            };
            var modified = ModifierEvaluator.Apply(entry, ctx);

            // Hidden entry error: if entry is hidden and has selections + categoryLinks
            if (hasCategoryLinks && modified.Hidden)
            {
                var hiddenCount = CountSelectionsInScope("selections", "parent", entryId, false, force, true);
                if (hiddenCount > 0)
                {
                    errors.Add(new ValidationErrorState(
                        Message: $"Entry {entryId} is hidden but has {hiddenCount} selection(s)",
                        OwnerType: "selection",
                        OwnerEntryId: entryId,
                        EntryId: entryId,
                        ConstraintId: "hidden"));
                }
            }

            if (entry.Constraints is null) continue;

            foreach (var constraint in entry.Constraints)
            {
                // field=forces on selection entry: always validate with roster owner
                if (constraint.Field == "forces")
                {
                    double forceCount = 0;
                    if (constraint.Scope == "roster")
                        forceCount = _forces.Count(f => f.ForceEntry.Id == entryId);
                    var constraintValue = modified.ConstraintValues.GetValueOrDefault(constraint.Id, constraint.Value);
                    CheckConstraint(constraint, constraintValue, forceCount, entryId,
                        "roster", null, errors, modified.Hidden);
                    continue;
                }

                // Determine which entryId to use for this constraint's error
                // Entry link constraints use the link's ID, shared entry constraints use target ID
                bool isLinkConstraint = avail.SourceLink?.Constraints?.Any(c => c.Id == constraint.Id) == true;
                string constraintEntryId = isLinkConstraint ? avail.SourceLink!.Id : entryId;

                // Shared constraint: validate once per (constraintId, entryId)
                if (constraint.Shared)
                {
                    if (!sharedChecked.Add((constraint.Id, entryId))) continue;
                    // For merged entries with both shared and link constraints,
                    // use the most restrictive value (BattleScribe behavior)
                    ValidateSharedConstraint(constraint, entry, modified, force, errors,
                        avail.SourceLink?.Constraints);
                    continue;
                }

                // Skip link constraints that have a matching shared constraint on the same entry
                // (the shared constraint handler absorbs the link's more restrictive value)
                if (isLinkConstraint)
                {
                    bool hasMatchingShared = entry.Constraints.Any(c =>
                        c.Shared && c.Field == constraint.Field && c.Type == constraint.Type);
                    if (hasMatchingShared) continue;
                }

                // Regular constraint
                var constraintVal = modified.ConstraintValues.GetValueOrDefault(constraint.Id, constraint.Value);
                double count;
                if (constraint.Field == "selections")
                {
                    count = CountSelectionsInScope(constraint.Field, constraint.Scope, entryId,
                        constraint.IncludeChildSelections, force, true);
                }
                else
                {
                    // Cost field
                    count = CountCostInScope(constraint.Field, constraint.Scope, entryId,
                        constraint.IncludeChildSelections, force, true);
                }

                double effectiveConstraintValue = constraintVal;
                if (constraint.PercentValue)
                {
                    var total = CountTotalSelectionsInScope(constraint.Scope, constraint.IncludeChildSelections, force);
                    effectiveConstraintValue = total * constraintVal / 100.0;
                }

                var (ot, oeid) = isLinkConstraint
                    ? ("selection", (string?)entryId)
                    : GetOwnerForConstraint(constraint, entry, entryId);
                CheckConstraint(constraint, effectiveConstraintValue, count, constraintEntryId,
                    ot, oeid, errors, modified.Hidden);
            }
        }

        // Validate child selection constraints
        foreach (var sel in force.Selections)
        {
            ValidateChildConstraints(sel, force, errors);
        }

        // Validate category constraints (from force entry's category links)
        ValidateCategoryConstraints(force, errors);
    }

    private void ValidateSharedConstraint(
        ProtocolConstraint constraint,
        ProtocolSelectionEntry sharedEntry,
        ModifiedProperties modified,
        RosterForce force,
        List<ValidationErrorState> errors,
        List<ProtocolConstraint>? linkConstraints = null)
    {
        // Count by shared entry ID (Entry.Id), not by effectiveId (link ID)
        string sharedEntryId = sharedEntry.Id;
        var constraintVal = modified.ConstraintValues.GetValueOrDefault(constraint.Id, constraint.Value);

        // For merged entries, absorb the most restrictive link constraint value
        // BattleScribe attributes error to the shared constraint but uses the more restrictive value
        if (linkConstraints is { Count: > 0 })
        {
            foreach (var lc in linkConstraints)
            {
                if (lc.Field == constraint.Field && lc.Type == constraint.Type)
                {
                    if (constraint.Type == "max" && lc.Value < constraintVal)
                        constraintVal = lc.Value;
                    else if (constraint.Type == "min" && lc.Value > constraintVal)
                        constraintVal = lc.Value;
                }
            }
        }

        double count;
        if (constraint.Field == "selections")
        {
            count = CountSelectionsInScope(constraint.Field, constraint.Scope, sharedEntryId,
                constraint.IncludeChildSelections, force, true);
        }
        else
        {
            count = CountCostInScope(constraint.Field, constraint.Scope, sharedEntryId,
                constraint.IncludeChildSelections, force, true);
        }

        double effectiveConstraintValue = constraintVal;
        if (constraint.PercentValue)
        {
            var total = CountTotalSelectionsInScope(constraint.Scope, constraint.IncludeChildSelections, force);
            effectiveConstraintValue = total * constraintVal / 100.0;
        }

        var (ot, oeid) = GetOwnerForConstraint(constraint, sharedEntry, sharedEntryId);
        CheckConstraint(constraint, effectiveConstraintValue, count, sharedEntryId,
            ot, oeid, errors, modified.Hidden);
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

                CheckConstraint(constraint, constraintValue, count, effectiveId,
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

    private static (string ownerType, string? ownerEntryId) GetOwnerForConstraint(
        ProtocolConstraint constraint, ProtocolSelectionEntry entry, string entryId)
    {
        // scope=force + type=min → force owner (force-level minimum requirement)
        if (constraint.Scope == "force" && constraint.Type == "min")
            return ("force", null);

        // min constraints with primary category → category owner
        if (constraint.Type == "min")
        {
            if (entry.CategoryLinks is { Count: > 0 })
            {
                var primary = entry.CategoryLinks.FirstOrDefault(cl => cl.Primary);
                if (primary != null)
                    return ("category", primary.TargetId);
                return ("category", entry.CategoryLinks[0].TargetId);
            }
        }

        // All other selection entry constraints → selection owner
        return ("selection", entryId);
    }

    private double CountSelectionsInScope(
        string field,
        string scope,
        string targetId,
        bool includeChildren,
        RosterForce force,
        bool matchByEntryId)
    {
        var selections = GetSelectionsInScope(scope, includeChildren, force);
        return selections
            .Where(s => matchByEntryId ? s.Entry.Id == targetId : ModifierEvaluator.GetEffectiveId(s) == targetId)
            .Sum(s => (double)s.Number);
    }

    private double CountCostInScope(
        string costField,
        string scope,
        string targetId,
        bool includeChildren,
        RosterForce force,
        bool matchByEntryId)
    {
        var selections = GetSelectionsInScope(scope, includeChildren, force);
        return selections
            .Where(s => matchByEntryId ? s.Entry.Id == targetId : ModifierEvaluator.GetEffectiveId(s) == targetId)
            .Sum(s => GetSelectionCostValue(s, costField));
    }

    private double CountTotalSelectionsInScope(string scope, bool includeChildren, RosterForce force)
    {
        var selections = GetSelectionsInScope(scope, includeChildren, force);
        return selections.Sum(s => (double)s.Number);
    }

    private IEnumerable<RosterSelection> GetSelectionsInScope(string scope, bool includeChildren, RosterForce force)
    {
        return scope switch
        {
            "parent" or "force" => includeChildren
                ? force.Selections.SelectMany(ModifierEvaluator.Flatten)
                : force.Selections,
            "roster" => _forces.SelectMany(f =>
                includeChildren
                    ? f.Selections.SelectMany(ModifierEvaluator.Flatten)
                    : f.Selections),
            _ => force.Selections,
        };
    }

    private static double GetSelectionCostValue(RosterSelection sel, string costTypeId)
    {
        if (sel.Entry.Costs is null) return 0;
        var cost = sel.Entry.Costs.FirstOrDefault(c => c.TypeId == costTypeId);
        if (cost is null) return 0;
        return sel.Entry.Collective ? cost.Value : cost.Value * sel.Number;
    }

    private static void CheckConstraint(
        ProtocolConstraint constraint,
        double constraintValue,
        double count,
        string entryId,
        string ownerType,
        string? ownerEntryId,
        List<ValidationErrorState> errors,
        bool isHidden)
    {
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
