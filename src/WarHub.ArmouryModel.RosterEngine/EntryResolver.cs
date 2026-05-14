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
public static class EntryResolver
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
    public static IReadOnlyList<AvailableEntry> GetAvailableEntries(ICatalogueSymbol catalogue)
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
    /// Counts root-level selection entries (excluding groups)
    /// for the <c>availableEntryCount</c> protocol field.
    /// </summary>
    public static int GetRootEntryCount(ICatalogueSymbol catalogue)
    {
        var count = CountRootSelectionEntries(catalogue.Gamesystem.RootContainerEntries);
        if (!catalogue.IsGamesystem)
        {
            count += CountRootSelectionEntries(catalogue.RootContainerEntries);
        }
        return count;
    }

    private static int CountRootSelectionEntries(ImmutableArray<IContainerEntrySymbol> rootEntries)
    {
        var count = 0;
        foreach (var entry in rootEntries)
        {
            if (entry is ISelectionEntryContainerSymbol sec)
            {
                // Resolve links to determine actual type; skip groups
                var resolved = ResolveEntry(sec);
                if (resolved is ISelectionEntryGroupSymbol)
                    continue;
                count++;
            }
        }
        return count;
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
    /// <param name="contextLinkPrefix">
    /// The link prefix context of <paramref name="entry"/> itself (from parent links above it).
    /// When <paramref name="entry"/> is itself a link, its ID is appended to this prefix
    /// to form the prefix propagated to its children.
    /// </param>
    /// <returns>An ordered, flattened list of child entries.</returns>
    public static IReadOnlyList<AvailableEntry> GetChildEntries(
        ISelectionEntryContainerSymbol entry,
        string contextLinkPrefix = "")
    {
        var result = new List<AvailableEntry>();

        // Resolve through links to get the effective container with children.
        var effective = ResolveEntry(entry);

        // If this entry is a link, children accumulate its ID into the prefix.
        var childPrefix = entry.ReferencedEntry is not null
            ? JoinLinkPrefix(contextLinkPrefix, entry.Id ?? "")
            : contextLinkPrefix;

        foreach (var child in effective.ChildSelectionEntries)
        {
            AddEntryOrFlatten(child, result, sourceGroup: null, parentLinkPrefix: childPrefix);
        }

        return result;
    }

    /// <summary>
    /// Filters <see cref="ICatalogueSymbol.RootContainerEntries"/> to selection entry
    /// containers and adds them to <paramref name="result"/>, flattening groups encountered.
    /// Root entries have no link prefix context (they are at the catalogue root level).
    /// </summary>
    private static void AddRootSelectionEntries(
        ImmutableArray<IContainerEntrySymbol> rootEntries,
        List<AvailableEntry> result)
    {
        foreach (var entry in rootEntries)
        {
            if (entry is ISelectionEntryContainerSymbol selEntry)
            {
                AddEntryOrFlatten(selEntry, result, sourceGroup: null, parentLinkPrefix: "");
            }
            // IForceEntrySymbol and ICategoryEntrySymbol are silently skipped.
        }
    }

    /// <summary>
    /// Adds a single concrete entry to the result, or recursively flattens a group.
    /// </summary>
    /// <param name="entry">The entry to add or flatten.</param>
    /// <param name="result">The result list to populate.</param>
    /// <param name="sourceGroup">The enclosing group (set on flattened children).</param>
    /// <param name="parentLinkPrefix">
    /// The accumulated link prefix from parent links above this entry.
    /// When <paramref name="entry"/> is itself a link to a group, its ID extends the prefix
    /// for the group's children.
    /// </param>
    private static void AddEntryOrFlatten(
        ISelectionEntryContainerSymbol entry,
        List<AvailableEntry> result,
        ISelectionEntryGroupSymbol? sourceGroup,
        string parentLinkPrefix = "")
    {
        var resolved = ResolveEntry(entry);

        if (resolved is ISelectionEntryGroupSymbol group)
        {
            // The target is a group — compute the prefix for the group's children.
            // If this entry is a link to the group, the link's ID is added to the prefix.
            var groupPrefix = entry.ReferencedEntry is not null
                ? JoinLinkPrefix(parentLinkPrefix, entry.Id ?? "")
                : parentLinkPrefix;
            FlattenGroup(group, result, visited: null, groupLinkPrefix: groupPrefix);
        }
        else
        {
            // Concrete entry (direct SelectionEntry or link-to-entry).
            result.Add(new AvailableEntry
            {
                Symbol = entry,
                SourceGroup = sourceGroup,
                LinkPrefix = parentLinkPrefix,
            });
        }
    }

    /// <summary>
    /// Recursively flattens a group's child entries into the result list.
    /// Each leaf entry records the originating <paramref name="group"/> as its
    /// <see cref="AvailableEntry.SourceGroup"/>.
    /// A visited-set prevents infinite recursion on self-referencing groups.
    /// </summary>
    /// <param name="group">The group to flatten.</param>
    /// <param name="result">The result list to populate.</param>
    /// <param name="visited">Set of visited group IDs (cycle guard).</param>
    /// <param name="groupLinkPrefix">
    /// The accumulated link prefix for entries inside this group.
    /// This includes the IDs of all links traversed to reach this group.
    /// </param>
    private static void FlattenGroup(
        ISelectionEntryGroupSymbol group,
        List<AvailableEntry> result,
        HashSet<string>? visited,
        string groupLinkPrefix = "")
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
                // Nested group: if the child is a link to the group, extend the prefix.
                var nestedPrefix = child.ReferencedEntry is not null
                    ? JoinLinkPrefix(groupLinkPrefix, child.Id ?? "")
                    : groupLinkPrefix;
                FlattenGroup(childGroup, result, visited, nestedPrefix);
            }
            else
            {
                result.Add(new AvailableEntry
                {
                    Symbol = child,
                    SourceGroup = group,
                    LinkPrefix = groupLinkPrefix,
                });
            }
        }
    }

    /// <summary>
    /// Joins a link prefix with an ID, using the "::" separator.
    /// When the prefix is empty, returns the ID directly.
    /// </summary>
    internal static string JoinLinkPrefix(string prefix, string id)
        => string.IsNullOrEmpty(prefix) ? id : $"{prefix}::{id}";

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

    /// <summary>
    /// Computes the full composite entryId for an available entry, incorporating any link prefix.
    /// For link entries: <c>linkPrefix::linkId::targetId</c>.
    /// For direct entries: <c>linkPrefix::entryId</c> (prefix may be empty).
    /// </summary>
    public static string ComputeCompositeEntryId(AvailableEntry avail)
    {
        var entry = avail.Symbol;
        var prefix = avail.LinkPrefix;
        if (entry.ReferencedEntry is { Id: { } targetId })
        {
            return JoinLinkPrefix(JoinLinkPrefix(prefix, entry.Id ?? ""), targetId);
        }
        return JoinLinkPrefix(prefix, entry.Id ?? "");
    }

    /// <summary>
    /// Finds an available entry by matching the definition entry ID.
    /// First tries to match against each entry's computed composite entryId
    /// (which incorporates any link prefix), then falls back to matching
    /// the entry's own ID or resolved target ID directly.
    /// </summary>
    public static AvailableEntry FindByEntryId(IReadOnlyList<AvailableEntry> available, string entryId)
    {
        foreach (var avail in available)
        {
            if (ComputeCompositeEntryId(avail) == entryId) return avail;
        }
        foreach (var avail in available)
        {
            if (avail.Symbol.Id == entryId) return avail;
        }
        foreach (var avail in available)
        {
            var resolved = avail.Symbol.ReferencedEntry ?? avail.Symbol;
            if (resolved.Id == entryId) return avail;
        }
        throw new ArgumentException(
            $"Entry '{entryId}' not found among {available.Count} available entries.",
            nameof(entryId));
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

    /// <summary>
    /// The accumulated link prefix from all link entries traversed above this entry.
    /// When non-empty, this prefix is prepended to the entry's own ID path to form
    /// the composite <c>entryId</c> written into the roster selection node.
    /// </summary>
    public string LinkPrefix { get; init; } = "";
}