namespace WarHub.ArmouryModel.RosterEngine;

/// <summary>
/// Resolves available entries from catalogue symbols for selection.
/// Flattens entry groups, resolves links via <see cref="IEntrySymbol.ReferencedEntry"/>,
/// and provides ordered entry lists matching the BattleScribe specification indexing.
/// </summary>
/// <remarks>
/// <para>
/// The resolver combines entries from the gamesystem (root catalogue) and a specific catalogue
/// to produce a flat, ordered list of selectable entries. The ordering within each catalogue
/// follows the <see cref="ICatalogueSymbol.RootContainerEntries"/> sequence which mirrors
/// the BattleScribe XML: direct selection entries first, then entry links.
/// </para>
/// <para>
/// Entry links are transparent — their <see cref="IEntrySymbol.ReferencedEntry"/> is used
/// to determine whether the link targets a concrete entry or a group.
/// Groups (including groups referenced via links) are recursively flattened:
/// each leaf entry within a group becomes a separate <see cref="AvailableEntry"/>
/// with the originating <see cref="AvailableEntry.SourceGroup"/> recorded.
/// </para>
/// </remarks>
public sealed class EntryResolver
{
    /// <summary>
    /// Gets the flattened list of available root entries for a catalogue.
    /// </summary>
    /// <param name="catalogue">
    /// The catalogue to resolve entries for. Entries from the catalogue's gamesystem
    /// (via <see cref="IModuleSymbol.Gamesystem"/>) are listed first, followed by
    /// entries from the catalogue itself (if it is not the gamesystem).
    /// </param>
    /// <returns>An ordered, flattened list of selectable entries.</returns>
    /// <remarks>
    /// Order within each source catalogue:
    /// <list type="number">
    ///   <item>Direct <see cref="ISelectionEntrySymbol"/> entries</item>
    ///   <item>Entry links resolving to concrete entries</item>
    ///   <item>Entry links resolving to groups — flattened into child entries</item>
    /// </list>
    /// Force entries (<see cref="IForceEntrySymbol"/>) and category entries
    /// (<see cref="ICategoryEntrySymbol"/>) present in
    /// <see cref="ICatalogueSymbol.RootContainerEntries"/> are excluded.
    /// </remarks>
    public IReadOnlyList<AvailableEntry> GetAvailableEntries(ICatalogueSymbol catalogue)
    {
        var result = new List<AvailableEntry>();

        // Phase 1: gamesystem root selection entries.
        var gsSym = catalogue.Gamesystem;
        AddRootSelectionEntries(gsSym.RootContainerEntries, result);

        // Phase 2: catalogue root selection entries (skip if the catalogue IS the gamesystem).
        if (!catalogue.IsGamesystem)
        {
            AddRootSelectionEntries(catalogue.RootContainerEntries, result);
        }

        return result;
    }

    /// <summary>
    /// Gets the flattened list of child entries under a selection entry container.
    /// Direct child entries and resolved links are added; child groups are recursively
    /// flattened so that the caller receives only concrete selectable entries.
    /// </summary>
    /// <param name="entry">
    /// The parent entry whose children to enumerate.
    /// If the entry is a link, it is resolved first via <see cref="ResolveEntry"/>.
    /// </param>
    /// <returns>An ordered, flattened list of child entries.</returns>
    public IReadOnlyList<AvailableEntry> GetChildEntries(ISelectionEntryContainerSymbol entry)
    {
        var result = new List<AvailableEntry>();

        // Resolve through links to get the effective container with children.
        var effective = ResolveEntry(entry);

        foreach (var child in effective.ChildSelectionEntries)
        {
            AddEntryOrFlatten(child, result, sourceGroup: null);
        }

        return result;
    }

    /// <summary>
    /// Filters <see cref="ICatalogueSymbol.RootContainerEntries"/> to selection entry
    /// containers and adds them to <paramref name="result"/>, flattening groups encountered.
    /// </summary>
    private static void AddRootSelectionEntries(
        ImmutableArray<IContainerEntrySymbol> rootEntries,
        List<AvailableEntry> result)
    {
        foreach (var entry in rootEntries)
        {
            if (entry is ISelectionEntryContainerSymbol selEntry)
            {
                AddEntryOrFlatten(selEntry, result, sourceGroup: null);
            }
            // IForceEntrySymbol and ICategoryEntrySymbol are silently skipped.
        }
    }

    /// <summary>
    /// Adds a single concrete entry to the result, or recursively flattens a group.
    /// </summary>
    private static void AddEntryOrFlatten(
        ISelectionEntryContainerSymbol entry,
        List<AvailableEntry> result,
        ISelectionEntryGroupSymbol? sourceGroup)
    {
        var resolved = ResolveEntry(entry);

        if (resolved is ISelectionEntryGroupSymbol group)
        {
            // The target is a group — flatten its children.
            FlattenGroup(group, result, visited: null);
        }
        else
        {
            // Concrete entry (direct SelectionEntry or link-to-entry).
            result.Add(new AvailableEntry
            {
                Symbol = entry,
                SourceGroup = sourceGroup,
            });
        }
    }

    /// <summary>
    /// Recursively flattens a group's child entries into the result list.
    /// Each leaf entry records the originating <paramref name="group"/> as its
    /// <see cref="AvailableEntry.SourceGroup"/>.
    /// A visited-set prevents infinite recursion on self-referencing groups.
    /// </summary>
    private static void FlattenGroup(
        ISelectionEntryGroupSymbol group,
        List<AvailableEntry> result,
        HashSet<string>? visited)
    {
        if (group.Id is not null)
        {
            visited ??= [];
            if (!visited.Add(group.Id)) return; // cycle guard
        }

        foreach (var child in group.ChildSelectionEntries)
        {
            var resolved = ResolveEntry(child);

            if (resolved is ISelectionEntryGroupSymbol childGroup)
            {
                FlattenGroup(childGroup, result, visited);
            }
            else
            {
                result.Add(new AvailableEntry
                {
                    Symbol = child,
                    SourceGroup = group,
                });
            }
        }
    }

    /// <summary>
    /// Follows the <see cref="ISelectionEntryContainerSymbol.ReferencedEntry"/> chain
    /// to resolve links to their ultimate concrete target (entry or group).
    /// A depth limit of 32 prevents runaway resolution on malformed data.
    /// </summary>
    internal static ISelectionEntryContainerSymbol ResolveEntry(ISelectionEntryContainerSymbol entry)
    {
        var current = entry;
        for (var depth = 0; depth < 32 && current.ReferencedEntry is { } referenced; depth++)
        {
            current = referenced;
        }
        return current;
    }
}

/// <summary>
/// Represents an available entry for selection, wrapping the catalogue symbol
/// and optionally tracking the group it was flattened from.
/// </summary>
public sealed class AvailableEntry
{
    /// <summary>
    /// The symbol representing this available entry.
    /// May be a direct <see cref="ISelectionEntrySymbol"/>, a link symbol
    /// (whose <see cref="ISelectionEntryContainerSymbol.ReferencedEntry"/> points to the
    /// concrete target), or a child entry from a flattened group.
    /// </summary>
    public required ISelectionEntryContainerSymbol Symbol { get; init; }

    /// <summary>
    /// The group this entry was flattened from, if any.
    /// Used to populate <c>entryGroupId</c> on the resulting
    /// <see cref="Source.SelectionNode"/>.
    /// </summary>
    public ISelectionEntryGroupSymbol? SourceGroup { get; init; }
}