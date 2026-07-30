using WarHub.ArmouryModel.EditorServices;
using WarHub.ArmouryModel.Source;

namespace WarHub.ArmouryModel.RosterEngine;

/// <summary>
/// Result of a roster mutation operation. Contains the new roster state plus metadata
/// about what was created, enabling callers to build protocol outputs without
/// diffing the entire roster tree.
/// </summary>
public sealed class MutationResult
{
    public MutationResult(RosterState state)
    {
        State = state;
    }

    /// <summary>
    /// The new roster state after the mutation.
    /// </summary>
    public RosterState State { get; }

    /// <summary>
    /// The ID of the newly created force (for AddForce/DuplicateForce operations).
    /// </summary>
    public string? ForceId { get; init; }

    /// <summary>
    /// The ID of the primary newly created selection (for SelectEntry/SelectChildEntry/DuplicateSelection).
    /// </summary>
    public string? SelectionId { get; init; }

    /// <summary>
    /// Map of entryId → selectionId for auto-created child selections.
    /// For "linkId::targetId" format entry IDs, the key is the targetId portion.
    /// Does not include the primary selection (identified by <see cref="SelectionId"/>).
    /// Null when no auto-created children exist.
    /// </summary>
    public Dictionary<string, string>? Selections { get; init; }

    /// <summary>
    /// Builds the <see cref="Selections"/> map from a created selection subtree,
    /// excluding the root selection itself (which is the primary).
    /// </summary>
    internal static Dictionary<string, string>? CollectSelectionMap(SelectionNode rootSelection)
    {
        Dictionary<string, string>? map = null;
        CollectSelectionsRecursive(rootSelection.Selections, ref map);
        return map;
    }

    /// <summary>
    /// Builds the <see cref="Selections"/> map from all selections in a force subtree,
    /// including the force's direct selections and nested forces.
    /// </summary>
    internal static Dictionary<string, string>? CollectSelectionMapFromForce(ForceNode force)
    {
        Dictionary<string, string>? map = null;
        CollectSelectionsRecursive(force.Selections, ref map);
        foreach (var childForce in force.Forces)
        {
            CollectSelectionMapFromForce(childForce, ref map);
        }
        return map;
    }

    private static void CollectSelectionMapFromForce(ForceNode force, ref Dictionary<string, string>? map)
    {
        CollectSelectionsRecursive(force.Selections, ref map);
        foreach (var childForce in force.Forces)
        {
            CollectSelectionMapFromForce(childForce, ref map);
        }
    }

    private static void CollectSelectionsRecursive(
        IReadOnlyList<SelectionNode> selections,
        ref Dictionary<string, string>? map)
    {
        foreach (var sel in selections)
        {
            var key = GetSelectionMapKey(sel);
            if (key is not null && sel.Id is not null)
            {
                map ??= new(StringComparer.Ordinal);
                map.TryAdd(key, sel.Id);
            }
            CollectSelectionsRecursive(sel.Selections, ref map);
        }
    }

    /// <summary>
    /// Gets the key for the selections map from a SelectionNode.
    /// For "linkId::targetId" format entryIds, returns the targetId.
    /// Otherwise returns the entryId directly.
    /// </summary>
    private static string? GetSelectionMapKey(SelectionNode sel)
    {
        var entryId = sel.EntryId;
        if (string.IsNullOrEmpty(entryId)) return null;
        var separatorIndex = entryId.IndexOf(WhamRosterEngine.EntryLinkIdSeparator, StringComparison.Ordinal);
        return separatorIndex >= 0 ? entryId[(separatorIndex + WhamRosterEngine.EntryLinkIdSeparator.Length)..] : entryId;
    }
}
