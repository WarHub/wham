using BattleScribeSpec;
using BattleScribeSpec.Protocol;
using WarHub.ArmouryModel.RosterEngine;

namespace WarHub.ArmouryModel.RosterEngine.Spec;

/// <summary>
/// Filters hidden constraint validation errors based on force provenance.
/// <para>
/// The constraint evaluator intentionally emits hidden-constraint violations for ALL
/// entries that are hidden but have selections. This is correct from a pure validation
/// standpoint. However, BattleScribe's roster-building workflow suppresses these errors
/// for forces that have only auto-selected entries (from AddForce) and no explicit
/// user selections (SelectEntry/SelectChildEntry).
/// </para>
/// <para>
/// This filter implements that suppression policy. It is the single source of truth
/// for the hidden-error filtering rule, replacing inline logic previously scattered
/// in the adapter.
/// </para>
/// </summary>
internal static class HiddenConstraintFilter
{
    private const string HiddenConstraintId = "hidden";
    /// <summary>
    /// Filters validation errors, suppressing hidden-constraint violations
    /// for forces that have not received explicit user selections.
    /// </summary>
    /// <param name="errors">All validation errors from the constraint evaluator.</param>
    /// <param name="forcesWithExplicitSelections">
    /// Set of force IDs that have received explicit SelectEntry/SelectChildEntry calls.
    /// Forces not in this set only have auto-selections from AddForce.
    /// </param>
    /// <param name="forces">The roster's force states for per-force analysis.</param>
    /// <returns>Filtered errors with hidden violations suppressed where appropriate.</returns>
    public static IReadOnlyList<ValidationErrorState> Apply(
        IReadOnlyList<ValidationErrorState> errors,
        IReadOnlySet<string> forcesWithExplicitSelections,
        IReadOnlyList<ForceState> forces)
    {
        // Fast path: no explicit selections anywhere → suppress all hidden errors
        if (forcesWithExplicitSelections.Count == 0)
        {
            return errors.Where(e => e.ConstraintId != HiddenConstraintId).ToList();
        }

        // Fast path: all forces have explicit selections → no filtering needed
        var allForceIds = new HashSet<string>(StringComparer.Ordinal);
        CollectForceIds(forces, allForceIds);
        if (forcesWithExplicitSelections.IsSupersetOf(allForceIds))
            return errors;

        // Per-force filtering: collect entry IDs only from forces with explicit selections.
        // Hidden errors are kept only if their entry belongs to a force with explicit selections.
        var entryIdsInExplicitForces = new HashSet<string>(StringComparer.Ordinal);
        CollectEntryIdsFromForces(forces, forcesWithExplicitSelections, entryIdsInExplicitForces);

        return errors.Where(e =>
        {
            if (e.ConstraintId != HiddenConstraintId) return true;
            if (e.EntryId is not { } eid) return true;
            return entryIdsInExplicitForces.Contains(eid);
        }).ToList();
    }

    private static void CollectForceIds(IReadOnlyList<ForceState> forces, HashSet<string> ids)
    {
        foreach (var force in forces)
        {
            if (force.Id is not null)
                ids.Add(force.Id);
            if (force.ChildForces is { Count: > 0 })
                CollectForceIds(force.ChildForces, ids);
        }
    }

    private static void CollectEntryIdsFromForces(
        IReadOnlyList<ForceState> forces,
        IReadOnlySet<string> explicitForceIds,
        HashSet<string> ids)
    {
        foreach (var force in forces)
        {
            if (force.Id is not null && explicitForceIds.Contains(force.Id))
            {
                CollectSelectionEntryIds(force.Selections, ids);
            }
            if (force.ChildForces is { Count: > 0 })
                CollectEntryIdsFromForces(force.ChildForces, explicitForceIds, ids);
        }
    }

    private static void CollectSelectionEntryIds(IReadOnlyList<SelectionState> selections, HashSet<string> ids)
    {
        foreach (var sel in selections)
        {
            var eid = sel.EntryId;
            if (!string.IsNullOrEmpty(eid))
            {
                ids.Add(eid);
                var sepIdx = eid.IndexOf(WhamRosterEngine.EntryLinkIdSeparator, StringComparison.Ordinal);
                if (sepIdx >= 0)
                    ids.Add(eid[(sepIdx + WhamRosterEngine.EntryLinkIdSeparator.Length)..]);
            }
            if (sel.Children is { Count: > 0 })
                CollectSelectionEntryIds(sel.Children, ids);
        }
    }
}
