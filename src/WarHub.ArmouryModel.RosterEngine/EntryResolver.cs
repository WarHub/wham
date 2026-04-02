using WarHub.ArmouryModel.Concrete;

namespace WarHub.ArmouryModel.RosterEngine;

/// <summary>
/// Resolves available entries from catalogue symbols for selection.
/// Produces ordered entry lists matching the BattleScribe spec index semantics:
/// GS entries → GS links → Cat entries → Cat links for root entries;
/// direct children → entry links → flattened groups for child entries.
/// </summary>
public sealed class EntryResolver
{
    /// <summary>
    /// Gets the flattened list of available root entries for a catalogue.
    /// Combines gamesystem root entries + catalogue root entries.
    /// </summary>
    /// <remarks>
    /// Ordering must match legacy Protocol-based engine for spec index compatibility:
    /// 1. Gamesystem SelectionEntries (IsReference = false)
    /// 2. Gamesystem EntryLinks (IsReference = true)
    /// 3. Catalogue SelectionEntries (IsReference = false)
    /// 4. Catalogue EntryLinks (IsReference = true)
    ///
    /// The ISymbol <see cref="ICatalogueSymbol.RootContainerEntries"/> already yields
    /// SelectionEntries before EntryLinks (see CatalogueBaseSymbol), so we just filter
    /// for <see cref="ISelectionEntryContainerSymbol"/> to exclude force/category entries.
    /// </remarks>
    public IReadOnlyList<AvailableEntry> GetAvailableEntries(
        ICatalogueSymbol gamesystem,
        ICatalogueSymbol catalogue)
    {
        var result = new List<AvailableEntry>();

        // Phase 1-2: Gamesystem root entries (entries first, then links — natural ISymbol order)
        AddRootSelectionEntries(gamesystem, result);

        // Phase 3-4: Catalogue root entries (same ordering)
        if (!ReferenceEquals(gamesystem, catalogue))
        {
            AddRootSelectionEntries(catalogue, result);
        }

        return result;
    }

    /// <summary>
    /// Gets the flattened list of child entries under a selection entry container.
    /// Groups are recursively flattened into their constituent entries.
    /// </summary>
    /// <remarks>
    /// Legacy ordering: direct SelectionEntries → EntryLinks → flattened SelectionEntryGroups.
    /// ISymbol <see cref="ISelectionEntryContainerSymbol.ChildSelectionEntries"/> ordering is
    /// EntryLinks → SelectionEntries → SelectionEntryGroups, so we must reorder.
    /// </remarks>
    public IReadOnlyList<AvailableEntry> GetChildEntries(ISelectionEntryContainerSymbol entry)
    {
        var result = new List<AvailableEntry>();

        // We need to reorder to match legacy: entries → links → groups
        // ISymbol ChildSelectionEntries has: links → entries → groups
        // So we iterate and bucket by type, then add in legacy order.

        var directEntries = new List<ISelectionEntryContainerSymbol>();
        var linkEntries = new List<ISelectionEntryContainerSymbol>();
        var groupEntries = new List<ISelectionEntryGroupSymbol>();

        foreach (var child in entry.ChildSelectionEntries)
        {
            if (child is ISelectionEntryGroupSymbol group)
            {
                groupEntries.Add(group);
            }
            else if (child.IsReference)
            {
                linkEntries.Add(child);
            }
            else
            {
                directEntries.Add(child);
            }
        }

        // Legacy order: direct entries first
        foreach (var e in directEntries)
        {
            result.Add(new AvailableEntry { Symbol = e });
        }

        // Then entry links (resolved — the ISymbol already resolves via ReferencedEntry)
        foreach (var e in linkEntries)
        {
            // Entry links to groups: add as group entry (will be a group symbol via ReferencedEntry)
            if (e.ReferencedEntry is ISelectionEntryGroupSymbol linkedGroup)
            {
                result.Add(new AvailableEntry { Symbol = e, SourceGroup = linkedGroup });
            }
            else
            {
                result.Add(new AvailableEntry { Symbol = e });
            }
        }

        // Then groups — flattened recursively
        foreach (var group in groupEntries)
        {
            FlattenGroup(group, result);
        }

        return result;
    }

    private static void AddRootSelectionEntries(ICatalogueSymbol catalogue, List<AvailableEntry> result)
    {
        foreach (var entry in catalogue.RootContainerEntries)
        {
            if (entry is ISelectionEntryContainerSymbol selContainer)
            {
                result.Add(new AvailableEntry { Symbol = selContainer });
            }
        }
    }

    /// <summary>
    /// Recursively flattens a group into its constituent entries.
    /// Each entry is tagged with the <paramref name="group"/> as its <see cref="AvailableEntry.SourceGroup"/>.
    /// Nested groups are recursively flattened; cycle detection prevents infinite loops.
    /// </summary>
    private static void FlattenGroup(
        ISelectionEntryGroupSymbol group,
        List<AvailableEntry> result,
        HashSet<string>? visited = null)
    {
        visited ??= [];
        if (!visited.Add(group.Id))
            return; // cycle detected

        foreach (var child in group.ChildSelectionEntries)
        {
            if (child is ISelectionEntryGroupSymbol subGroup)
            {
                // Nested group — recurse
                FlattenGroup(subGroup, result, visited);
            }
            else
            {
                // Entry or link from within a group — tag with source group
                result.Add(new AvailableEntry { Symbol = child, SourceGroup = group });
            }
        }
    }
}

/// <summary>
/// An available entry for selection. Wraps an <see cref="ISelectionEntryContainerSymbol"/>
/// and optionally tracks the source group it was flattened from (for category inheritance).
/// </summary>
public sealed class AvailableEntry
{
    /// <summary>The symbol representing the available entry.</summary>
    public required ISelectionEntryContainerSymbol Symbol { get; init; }

    /// <summary>
    /// The group this entry was flattened from, if any.
    /// Used to inherit category links from the containing group onto the selection.
    /// </summary>
    public ISelectionEntryGroupSymbol? SourceGroup { get; init; }
}
