using BattleScribeSpec.Protocol;

namespace WarHub.ArmouryModel.RosterEngine;

/// <summary>
/// Resolves entry references, flattens available entries, and merges entry links with targets.
/// </summary>
internal sealed class EntryResolver
{
    private readonly ProtocolGameSystem _gameSystem;
    private readonly ProtocolCatalogue[] _catalogues;
    private readonly Dictionary<string, ProtocolSelectionEntry> _sharedEntries = new(StringComparer.Ordinal);
    private readonly Dictionary<string, ProtocolSelectionEntryGroup> _sharedGroups = new(StringComparer.Ordinal);
    private readonly Dictionary<string, ProtocolRule> _sharedRules = new(StringComparer.Ordinal);
    private readonly Dictionary<string, ProtocolProfile> _sharedProfiles = new(StringComparer.Ordinal);
    private readonly Dictionary<string, ProtocolInfoGroup> _sharedInfoGroups = new(StringComparer.Ordinal);

    public EntryResolver(ProtocolGameSystem gameSystem, ProtocolCatalogue[] catalogues)
    {
        _gameSystem = gameSystem;
        _catalogues = catalogues;
        IndexSharedEntries();
    }

    private void IndexSharedEntries()
    {
        // Index game system shared entries
        IndexShared(_gameSystem.SharedSelectionEntries, _sharedEntries, e => e.Id);
        IndexShared(_gameSystem.SharedSelectionEntryGroups, _sharedGroups, e => e.Id);
        IndexShared(_gameSystem.SharedRules, _sharedRules, e => e.Id);
        IndexShared(_gameSystem.SharedProfiles, _sharedProfiles, e => e.Id);
        IndexShared(_gameSystem.SharedInfoGroups, _sharedInfoGroups, e => e.Id);

        // Index catalogue shared entries (catalogue overrides game system)
        foreach (var cat in _catalogues)
        {
            IndexShared(cat.SharedSelectionEntries, _sharedEntries, e => e.Id);
            IndexShared(cat.SharedSelectionEntryGroups, _sharedGroups, e => e.Id);
            IndexShared(cat.SharedRules, _sharedRules, e => e.Id);
            IndexShared(cat.SharedProfiles, _sharedProfiles, e => e.Id);
            IndexShared(cat.SharedInfoGroups, _sharedInfoGroups, e => e.Id);
        }
    }

    private static void IndexShared<T>(List<T>? items, Dictionary<string, T> dict, Func<T, string> getId)
    {
        if (items is null) return;
        foreach (var item in items)
        {
            dict[getId(item)] = item;
        }
    }

    /// <summary>
    /// Get the flattened list of available root entries for a catalogue.
    /// Order: selectionEntries, then entryLinks (resolved), then selectionEntryGroups.
    /// Each element is a resolved ProtocolSelectionEntry or ProtocolSelectionEntryGroup
    /// wrapped as an AvailableEntry.
    /// </summary>
    public List<AvailableEntry> GetAvailableEntries(ProtocolCatalogue catalogue)
    {
        var result = new List<AvailableEntry>();

        // Direct selection entries
        if (catalogue.SelectionEntries is { } entries)
        {
            foreach (var entry in entries)
            {
                result.Add(new AvailableEntry { Entry = entry });
            }
        }

        // Entry links (resolve to shared entries)
        if (catalogue.EntryLinks is { } links)
        {
            foreach (var link in links)
            {
                if (link.Type == "selectionEntry" && _sharedEntries.TryGetValue(link.TargetId, out var target))
                {
                    var merged = MergeEntryLink(link, target);
                    result.Add(new AvailableEntry { Entry = merged, SourceLink = link });
                }
                else if (link.Type == "selectionEntryGroup" && _sharedGroups.TryGetValue(link.TargetId, out var groupTarget))
                {
                    // Entry link pointing to a group - flatten the group's entries
                    result.Add(new AvailableEntry { Group = groupTarget, SourceLink = link });
                }
            }
        }

        // Selection entry groups at root level
        if (catalogue.SelectionEntryGroups is { } groups)
        {
            foreach (var group in groups)
            {
                result.Add(new AvailableEntry { Group = group });
            }
        }

        return result;
    }

    /// <summary>
    /// Get the flattened list of child entries under a selection entry.
    /// </summary>
    public List<AvailableEntry> GetChildEntries(ProtocolSelectionEntry entry)
    {
        var result = new List<AvailableEntry>();

        if (entry.SelectionEntries is { } entries)
        {
            foreach (var child in entries)
                result.Add(new AvailableEntry { Entry = child });
        }

        if (entry.EntryLinks is { } links)
        {
            foreach (var link in links)
            {
                if (link.Type == "selectionEntry" && _sharedEntries.TryGetValue(link.TargetId, out var target))
                {
                    result.Add(new AvailableEntry { Entry = MergeEntryLink(link, target), SourceLink = link });
                }
                else if (link.Type == "selectionEntryGroup" && _sharedGroups.TryGetValue(link.TargetId, out var groupTarget))
                {
                    result.Add(new AvailableEntry { Group = groupTarget, SourceLink = link });
                }
            }
        }

        if (entry.SelectionEntryGroups is { } groups)
        {
            foreach (var group in groups)
                result.Add(new AvailableEntry { Group = group });
        }

        return result;
    }

    /// <summary>
    /// Merge an entry link with its target entry. Link properties override target properties.
    /// </summary>
    public static ProtocolSelectionEntry MergeEntryLink(ProtocolEntryLink link, ProtocolSelectionEntry target)
    {
        var merged = new ProtocolSelectionEntry
        {
            Id = target.Id,
            Name = !string.IsNullOrEmpty(link.Name) ? link.Name : target.Name,
            Type = target.Type,
            Hidden = link.Hidden || target.Hidden,
            Collective = link.Collective || target.Collective,
            Import = link.Import,
            Page = link.Page ?? target.Page,
            PublicationId = link.PublicationId ?? target.PublicationId,
            // Link costs override target costs if specified
            Costs = link.Costs is { Count: > 0 } ? link.Costs : target.Costs,
        };

        // Merge constraints: target + link
        merged.Constraints = MergeLists(target.Constraints, link.Constraints);
        merged.Modifiers = MergeLists(target.Modifiers, link.Modifiers);
        merged.ModifierGroups = MergeLists(target.ModifierGroups, link.ModifierGroups);
        merged.CategoryLinks = MergeLists(target.CategoryLinks, link.CategoryLinks);
        merged.Profiles = MergeLists(target.Profiles, link.Profiles);
        merged.Rules = MergeLists(target.Rules, link.Rules);
        merged.InfoGroups = MergeLists(target.InfoGroups, link.InfoGroups);
        merged.InfoLinks = MergeLists(target.InfoLinks, link.InfoLinks);

        // Children: target children + link supplementary children
        merged.SelectionEntries = MergeLists(target.SelectionEntries, link.SelectionEntries);
        merged.SelectionEntryGroups = MergeLists(target.SelectionEntryGroups, link.SelectionEntryGroups);
        merged.EntryLinks = MergeLists(target.EntryLinks, link.EntryLinks);

        return merged;
    }

    private static List<T>? MergeLists<T>(List<T>? first, List<T>? second)
    {
        if (first is null or { Count: 0 }) return second;
        if (second is null or { Count: 0 }) return first;
        return [.. first, .. second];
    }

    public ProtocolSelectionEntry? ResolveSharedEntry(string id) =>
        _sharedEntries.GetValueOrDefault(id);

    public ProtocolSelectionEntryGroup? ResolveSharedGroup(string id) =>
        _sharedGroups.GetValueOrDefault(id);
}

/// <summary>
/// Represents an available entry for selection. Could be a direct entry or a group.
/// </summary>
internal sealed class AvailableEntry
{
    public ProtocolSelectionEntry? Entry { get; init; }
    public ProtocolSelectionEntryGroup? Group { get; init; }
    public ProtocolEntryLink? SourceLink { get; init; }

    public bool IsGroup => Group is not null;
    public string Name => Entry?.Name ?? Group?.Name ?? "";
    public string Id => Entry?.Id ?? Group?.Id ?? "";
}
