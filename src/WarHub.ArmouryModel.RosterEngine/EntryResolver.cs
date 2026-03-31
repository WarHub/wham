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

        // Also index game system root-level rules (resolvable by InfoLinks)
        IndexShared(_gameSystem.Rules, _sharedRules, e => e.Id);

        // Index catalogue shared entries (catalogue overrides game system)
        foreach (var cat in _catalogues)
        {
            IndexShared(cat.SharedSelectionEntries, _sharedEntries, e => e.Id);
            IndexShared(cat.SharedSelectionEntryGroups, _sharedGroups, e => e.Id);
            IndexShared(cat.SharedRules, _sharedRules, e => e.Id);
            IndexShared(cat.SharedProfiles, _sharedProfiles, e => e.Id);
            IndexShared(cat.SharedInfoGroups, _sharedInfoGroups, e => e.Id);

            // Also index catalogue root-level rules
            IndexShared(cat.Rules, _sharedRules, e => e.Id);
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
    /// Includes game system root entries, catalogue entries/links, and groups.
    /// </summary>
    public List<AvailableEntry> GetAvailableEntries(ProtocolCatalogue catalogue)
    {
        var result = new List<AvailableEntry>();

        // Game system root selection entries
        if (_gameSystem.SelectionEntries is { } gsEntries)
        {
            foreach (var entry in gsEntries)
                result.Add(new AvailableEntry { Entry = entry });
        }

        // Game system entry links
        if (_gameSystem.EntryLinks is { } gsLinks)
        {
            foreach (var link in gsLinks)
            {
                if (link.Type == "selectionEntry" && _sharedEntries.TryGetValue(link.TargetId, out var target))
                    result.Add(new AvailableEntry { Entry = MergeEntryLink(link, target), SourceLink = link });
                else if (link.Type == "selectionEntryGroup" && _sharedGroups.TryGetValue(link.TargetId, out var groupTarget))
                    result.Add(new AvailableEntry { Group = groupTarget, SourceLink = link });
            }
        }

        // Catalogue direct selection entries
        if (catalogue.SelectionEntries is { } entries)
        {
            foreach (var entry in entries)
                result.Add(new AvailableEntry { Entry = entry });
        }

        // Catalogue entry links (resolve to shared entries)
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
                    result.Add(new AvailableEntry { Group = groupTarget, SourceLink = link });
                }
            }
        }

        // Selection entry groups at root level
        if (catalogue.SelectionEntryGroups is { } groups)
        {
            foreach (var group in groups)
                result.Add(new AvailableEntry { Group = group });
        }

        return result;
    }

    /// <summary>
    /// Get combined force entries from game system + catalogue.
    /// </summary>
    public ProtocolForceEntry GetForceEntry(int index, ProtocolCatalogue catalogue)
    {
        var gsEntries = _gameSystem.ForceEntries;
        var gsCount = gsEntries?.Count ?? 0;

        if (index < gsCount)
            return gsEntries![index];

        var catEntries = catalogue.ForceEntries;
        var adjustedIndex = index - gsCount;
        if (catEntries is not null && adjustedIndex < catEntries.Count)
            return catEntries[adjustedIndex];

        throw new ArgumentOutOfRangeException(nameof(index),
            $"Force entry index {index} out of range (gs={gsCount}, cat={catEntries?.Count ?? 0})");
    }

    /// <summary>
    /// Get the flattened list of child entries under a selection entry.
    /// For group-type entries, looks into the group's children.
    /// </summary>
    public List<AvailableEntry> GetChildEntries(ProtocolSelectionEntry entry)
    {
        var result = new List<AvailableEntry>();

        // Direct child entries
        if (entry.SelectionEntries is { } entries)
        {
            foreach (var child in entries)
                result.Add(new AvailableEntry { Entry = child });
        }

        // Child entry links
        if (entry.EntryLinks is { } links)
        {
            foreach (var link in links)
            {
                if (link.Type == "selectionEntry" && _sharedEntries.TryGetValue(link.TargetId, out var target))
                    result.Add(new AvailableEntry { Entry = MergeEntryLink(link, target), SourceLink = link });
                else if (link.Type == "selectionEntryGroup" && _sharedGroups.TryGetValue(link.TargetId, out var groupTarget))
                    result.Add(new AvailableEntry { Group = groupTarget, SourceLink = link });
            }
        }

        // Child selection entry groups - flatten into their children
        if (entry.SelectionEntryGroups is { } groups)
        {
            foreach (var group in groups)
                FlattenGroupChildren(group, result);
        }

        return result;
    }

    /// <summary>
    /// Flatten a group's entries, resolving nested groups recursively.
    /// </summary>
    private void FlattenGroupChildren(ProtocolSelectionEntryGroup group, List<AvailableEntry> result)
    {
        if (group.SelectionEntries is { } entries)
        {
            foreach (var entry in entries)
                result.Add(new AvailableEntry { Entry = entry, SourceGroup = group });
        }

        if (group.EntryLinks is { } links)
        {
            foreach (var link in links)
            {
                if (link.Type == "selectionEntry" && _sharedEntries.TryGetValue(link.TargetId, out var target))
                    result.Add(new AvailableEntry { Entry = MergeEntryLink(link, target), SourceLink = link, SourceGroup = group });
                else if (link.Type == "selectionEntryGroup" && _sharedGroups.TryGetValue(link.TargetId, out var groupTarget))
                    FlattenGroupChildren(groupTarget, result);
            }
        }

        if (group.SelectionEntryGroups is { } subGroups)
        {
            foreach (var subGroup in subGroups)
                FlattenGroupChildren(subGroup, result);
        }
    }

    // ===== InfoLink / InfoGroup resolution =====

    /// <summary>
    /// Resolve all profiles for an entry including InfoLinks and InfoGroups.
    /// </summary>
    public List<ProtocolProfile> ResolveAllProfiles(ProtocolSelectionEntry entry)
    {
        var result = new List<ProtocolProfile>();

        if (entry.Profiles is { } profiles)
            result.AddRange(profiles);

        if (entry.InfoLinks is { } infoLinks)
        {
            foreach (var link in infoLinks)
            {
                if (link.Type == "profile" && _sharedProfiles.TryGetValue(link.TargetId, out var target))
                    result.Add(CloneProfileWithOverrides(target, link));
                else if (link.Type == "infoGroup" && _sharedInfoGroups.TryGetValue(link.TargetId, out var group))
                    CollectInfoGroupProfiles(group, result);
            }
        }

        if (entry.InfoGroups is { } infoGroups)
        {
            foreach (var group in infoGroups)
                CollectInfoGroupProfiles(group, result);
        }

        return result;
    }

    /// <summary>
    /// Resolve all rules for an entry including InfoLinks and InfoGroups.
    /// </summary>
    public List<ProtocolRule> ResolveAllRules(ProtocolSelectionEntry entry)
    {
        var result = new List<ProtocolRule>();

        if (entry.Rules is { } rules)
            result.AddRange(rules);

        if (entry.InfoLinks is { } infoLinks)
        {
            foreach (var link in infoLinks)
            {
                if (link.Type == "rule" && _sharedRules.TryGetValue(link.TargetId, out var target))
                    result.Add(CloneRuleWithOverrides(target, link));
                else if (link.Type == "infoGroup" && _sharedInfoGroups.TryGetValue(link.TargetId, out var group))
                    CollectInfoGroupRules(group, result);
            }
        }

        if (entry.InfoGroups is { } infoGroups)
        {
            foreach (var group in infoGroups)
                CollectInfoGroupRules(group, result);
        }

        return result;
    }

    /// <summary>
    /// Resolve all profiles from a ForceEntry.
    /// </summary>
    public List<ProtocolProfile> ResolveForceEntryProfiles(ProtocolForceEntry forceEntry)
    {
        var result = new List<ProtocolProfile>();

        if (forceEntry.Profiles is { } profiles)
            result.AddRange(profiles);

        if (forceEntry.InfoLinks is { } infoLinks)
        {
            foreach (var link in infoLinks)
            {
                if (link.Type == "profile" && _sharedProfiles.TryGetValue(link.TargetId, out var target))
                    result.Add(CloneProfileWithOverrides(target, link));
                else if (link.Type == "infoGroup" && _sharedInfoGroups.TryGetValue(link.TargetId, out var group))
                    CollectInfoGroupProfiles(group, result);
            }
        }

        if (forceEntry.InfoGroups is { } infoGroups)
        {
            foreach (var group in infoGroups)
                CollectInfoGroupProfiles(group, result);
        }

        return result;
    }

    /// <summary>
    /// Resolve all rules from a ForceEntry.
    /// </summary>
    public List<ProtocolRule> ResolveForceEntryRules(ProtocolForceEntry forceEntry)
    {
        var result = new List<ProtocolRule>();

        if (forceEntry.Rules is { } rules)
            result.AddRange(rules);

        if (forceEntry.InfoLinks is { } infoLinks)
        {
            foreach (var link in infoLinks)
            {
                if (link.Type == "rule" && _sharedRules.TryGetValue(link.TargetId, out var target))
                    result.Add(CloneRuleWithOverrides(target, link));
                else if (link.Type == "infoGroup" && _sharedInfoGroups.TryGetValue(link.TargetId, out var group))
                    CollectInfoGroupRules(group, result);
            }
        }

        if (forceEntry.InfoGroups is { } infoGroups)
        {
            foreach (var group in infoGroups)
                CollectInfoGroupRules(group, result);
        }

        return result;
    }

    private void CollectInfoGroupProfiles(ProtocolInfoGroup group, List<ProtocolProfile> result)
    {
        if (group.Profiles is { } profiles)
        {
            foreach (var profile in profiles)
            {
                // Merge infoGroup's modifiers into each profile
                if (group.Modifiers is { Count: > 0 } || group.ModifierGroups is { Count: > 0 })
                {
                    result.Add(new ProtocolProfile
                    {
                        Id = profile.Id,
                        Name = profile.Name,
                        TypeId = profile.TypeId,
                        TypeName = profile.TypeName,
                        Hidden = profile.Hidden,
                        Page = profile.Page,
                        PublicationId = profile.PublicationId,
                        Characteristics = profile.Characteristics,
                        Modifiers = MergeLists(profile.Modifiers, group.Modifiers),
                        ModifierGroups = MergeLists(profile.ModifierGroups, group.ModifierGroups),
                    });
                }
                else
                {
                    result.Add(profile);
                }
            }
        }

        if (group.InfoLinks is { } infoLinks)
        {
            foreach (var link in infoLinks)
            {
                if (link.Type == "profile" && _sharedProfiles.TryGetValue(link.TargetId, out var target))
                    result.Add(CloneProfileWithOverrides(target, link));
                else if (link.Type == "infoGroup" && _sharedInfoGroups.TryGetValue(link.TargetId, out var group2))
                    CollectInfoGroupProfiles(group2, result);
            }
        }

        if (group.InfoGroups is { } nestedGroups)
        {
            foreach (var nested in nestedGroups)
                CollectInfoGroupProfiles(nested, result);
        }
    }

    private void CollectInfoGroupRules(ProtocolInfoGroup group, List<ProtocolRule> result)
    {
        if (group.Rules is { } rules)
        {
            foreach (var rule in rules)
            {
                if (group.Modifiers is { Count: > 0 } || group.ModifierGroups is { Count: > 0 })
                {
                    result.Add(new ProtocolRule
                    {
                        Id = rule.Id,
                        Name = rule.Name,
                        Description = rule.Description,
                        Hidden = rule.Hidden,
                        Page = rule.Page,
                        PublicationId = rule.PublicationId,
                        Modifiers = MergeLists(rule.Modifiers, group.Modifiers),
                        ModifierGroups = MergeLists(rule.ModifierGroups, group.ModifierGroups),
                    });
                }
                else
                {
                    result.Add(rule);
                }
            }
        }

        if (group.InfoLinks is { } infoLinks)
        {
            foreach (var link in infoLinks)
            {
                if (link.Type == "rule" && _sharedRules.TryGetValue(link.TargetId, out var target))
                    result.Add(CloneRuleWithOverrides(target, link));
                else if (link.Type == "infoGroup" && _sharedInfoGroups.TryGetValue(link.TargetId, out var group2))
                    CollectInfoGroupRules(group2, result);
            }
        }

        if (group.InfoGroups is { } nestedGroups)
        {
            foreach (var nested in nestedGroups)
                CollectInfoGroupRules(nested, result);
        }
    }

    private static ProtocolProfile CloneProfileWithOverrides(ProtocolProfile target, ProtocolInfoLink link)
    {
        return new ProtocolProfile
        {
            Id = target.Id,
            Name = !string.IsNullOrEmpty(link.Name) ? link.Name : target.Name,
            TypeId = target.TypeId,
            TypeName = target.TypeName,
            Hidden = link.Hidden || target.Hidden,
                        Page = target.Page,
            PublicationId = target.PublicationId,
            Characteristics = target.Characteristics,
            Modifiers = MergeLists(target.Modifiers, link.Modifiers),
            ModifierGroups = MergeLists(target.ModifierGroups, link.ModifierGroups),
        };
    }

    private static ProtocolRule CloneRuleWithOverrides(ProtocolRule target, ProtocolInfoLink link)
    {
        return new ProtocolRule
        {
            Id = target.Id,
            Name = !string.IsNullOrEmpty(link.Name) ? link.Name : target.Name,
            Description = target.Description,
            Hidden = link.Hidden || target.Hidden,
                        Page = target.Page,
            PublicationId = target.PublicationId,
            Modifiers = MergeLists(target.Modifiers, link.Modifiers),
            ModifierGroups = MergeLists(target.ModifierGroups, link.ModifierGroups),
        };
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

    public ProtocolRule? ResolveSharedRule(string id) =>
        _sharedRules.GetValueOrDefault(id);

    public ProtocolProfile? ResolveSharedProfile(string id) =>
        _sharedProfiles.GetValueOrDefault(id);

    public ProtocolInfoGroup? ResolveSharedInfoGroup(string id) =>
        _sharedInfoGroups.GetValueOrDefault(id);
}

/// <summary>
/// Represents an available entry for selection. Could be a direct entry or a group.
/// </summary>
internal sealed class AvailableEntry
{
    public ProtocolSelectionEntry? Entry { get; init; }
    public ProtocolSelectionEntryGroup? Group { get; init; }
    public ProtocolEntryLink? SourceLink { get; init; }
    public ProtocolSelectionEntryGroup? SourceGroup { get; init; }

    public bool IsGroup => Group is not null;
    public string Name => Entry?.Name ?? Group?.Name ?? "";
    public string Id => Entry?.Id ?? Group?.Id ?? "";
}

