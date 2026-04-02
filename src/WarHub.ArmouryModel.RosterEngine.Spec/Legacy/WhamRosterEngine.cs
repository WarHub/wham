using BattleScribeSpec;
using BattleScribeSpec.Protocol;

namespace WarHub.ArmouryModel.RosterEngine.Spec.Legacy;

/// <summary>
/// BattleScribe-spec conformant roster engine implementation.
/// Works directly with the protocol types for simplicity.
/// </summary>
public sealed class WhamRosterEngine : IRosterEngine
{
    private ProtocolGameSystem _gameSystem = new();
    private ProtocolCatalogue[] _catalogues = [];
    private EntryResolver _resolver = null!;
    private readonly List<RosterForce> _forces = [];
    private readonly Dictionary<string, double> _costLimits = new(StringComparer.Ordinal);

    public IReadOnlyList<string> Setup(ProtocolGameSystem gameSystem, ProtocolCatalogue[] catalogues)
    {
        _gameSystem = gameSystem;
        _catalogues = catalogues;
        _forces.Clear();
        _costLimits.Clear();
        _resolver = new EntryResolver(gameSystem, catalogues);

        if (gameSystem.CostTypes is { } costTypes)
        {
            foreach (var ct in costTypes)
            {
                if (ct.DefaultCostLimit is { } limit)
                    _costLimits[ct.Id] = limit;
            }
        }

        return [];
    }

    public void AddForce(int forceEntryIndex, int catalogueIndex = 0)
    {
        var catalogue = _catalogues[catalogueIndex];
        var forceEntry = _resolver.GetForceEntry(forceEntryIndex, catalogue);

        var force = new RosterForce
        {
            ForceEntry = forceEntry,
            Catalogue = catalogue
        };

        _forces.Add(force);
        AutoSelectEntries(force);
    }

    public void RemoveForce(int forceIndex)
    {
        _forces.RemoveAt(forceIndex);
    }

    public void SelectEntry(int forceIndex, int entryIndex)
    {
        var force = _forces[forceIndex];
        var available = _resolver.GetAvailableEntries(force.Catalogue);
        var avail = available[entryIndex];

        var selection = CreateSelection(avail);
        AutoSelectChildren(selection, force);
        force.Selections.Add(selection);
    }

    public void SelectChildEntry(int forceIndex, int selectionIndex, int childEntryIndex)
    {
        var force = _forces[forceIndex];
        var parentSelection = force.Selections[selectionIndex];
        var childEntries = _resolver.GetChildEntries(parentSelection.Entry);
        var avail = childEntries[childEntryIndex];

        var selection = CreateSelection(avail);
        parentSelection.Children.Add(selection);
    }

    public void DeselectSelection(int forceIndex, int selectionIndex)
    {
        var force = _forces[forceIndex];
        force.Selections.RemoveAt(selectionIndex);
    }

    public void SetSelectionCount(int forceIndex, int entryIndex, int count)
    {
        // No-op for root/force-level entries.
        // Root entries create new selections via selectEntry, not via count.
    }

    public void DuplicateSelection(int forceIndex, int selectionIndex)
    {
        var force = _forces[forceIndex];
        var original = force.Selections[selectionIndex];

        var duplicate = new RosterSelection
        {
            Entry = original.Entry,
            SourceLink = original.SourceLink,
            SourceGroup = original.SourceGroup,
            Number = 1
        };

        CopyChildren(original.Children, duplicate.Children);
        force.Selections.Add(duplicate);
    }

    public void SetCostLimit(string costTypeId, double value)
    {
        _costLimits[costTypeId] = value;
    }

    public RosterState GetRosterState()
    {
        var evaluator = CreateEvaluator();
        var forces = new List<ForceState>();

        foreach (var force in _forces)
        {
            var selections = new List<SelectionState>();
            foreach (var sel in force.Selections)
            {
                selections.Add(BuildSelectionState(sel, force, evaluator));
            }

            var forceProfiles = BuildForceProfiles(force.ForceEntry);
            var forceRules = BuildForceRules(force.ForceEntry);

            forces.Add(new ForceState(
                Name: force.ForceEntry.Name,
                CatalogueId: force.Catalogue.Id,
                Selections: selections,
                AvailableEntryCount: _resolver.GetAvailableEntries(force.Catalogue).Count,
                PublicationId: force.ForceEntry.PublicationId,
                Page: force.ForceEntry.Page
            )
            {
                Profiles = forceProfiles.Count > 0 ? forceProfiles : [],
                Rules = forceRules.Count > 0 ? forceRules : [],
            });
        }

        var costs = AggregateTotalCosts(evaluator);
        var errors = GetValidationErrors();

        return new RosterState(
            Name: "New Roster",
            GameSystemId: _gameSystem.Id,
            Forces: forces,
            Costs: costs,
            ValidationErrors: errors
        );
    }

    public IReadOnlyList<ValidationErrorState> GetValidationErrors()
    {
        var evaluator = CreateEvaluator();
        var validator = new ConstraintValidator(_gameSystem, _forces, evaluator, _costLimits, _resolver);
        return validator.Validate();
    }

    private ModifierEvaluator CreateEvaluator() => new(_gameSystem, _forces);

    private void AutoSelectEntries(RosterForce force)
    {
        var available = _resolver.GetAvailableEntries(force.Catalogue);
        var evaluator = CreateEvaluator();

        foreach (var avail in available)
        {
            if (avail.Entry is not { } entry) continue;
            if (entry.Constraints is not { } constraints) continue;

            foreach (var constraint in constraints)
            {
                if (constraint.Type != "min" || constraint.Field != "selections") continue;
                if (constraint.Scope != "parent" && constraint.Scope != "force") continue;

                var effectiveValue = evaluator.GetEffectiveConstraintValue(constraint, entry, null, force);
                if (effectiveValue < 1) continue;
                if (constraint.PercentValue) continue;

                // Hidden entries ARE auto-selected (constraints enforced)
                for (int i = 0; i < (int)effectiveValue; i++)
                {
                    var selection = CreateSelection(avail);
                    AutoSelectChildren(selection, force);
                    force.Selections.Add(selection);
                }
                break;
            }
        }
    }

    private void AutoSelectChildren(RosterSelection selection, RosterForce force)
    {
        var childEntries = _resolver.GetChildEntries(selection.Entry);
        var evaluator = CreateEvaluator();

        foreach (var avail in childEntries)
        {
            if (avail.Entry is not { } entry) continue;
            if (entry.Constraints is not { } constraints) continue;

            foreach (var constraint in constraints)
            {
                if (constraint.Type != "min" || constraint.Field != "selections" || constraint.Scope != "parent")
                    continue;

                var effectiveValue = evaluator.GetEffectiveConstraintValue(constraint, entry, null, force);
                if (effectiveValue < 1) continue;
                if (constraint.PercentValue) continue;

                // Hidden entries ARE auto-selected (constraints enforced)
                var childSel = CreateSelection(avail);
                childSel.Number = (int)effectiveValue;
                AutoSelectChildren(childSel, force);
                selection.Children.Add(childSel);
                break;
            }
        }
    }

    private static RosterSelection CreateSelection(AvailableEntry avail)
    {
        if (avail.Entry is { } entry)
        {
            // If entry comes from a group with categoryLinks, inherit them
            var effectiveEntry = entry;
            if (avail.SourceGroup?.CategoryLinks is { Count: > 0 } groupCatLinks)
            {
                effectiveEntry = new ProtocolSelectionEntry
                {
                    Id = entry.Id,
                    Name = entry.Name,
                    Type = entry.Type,
                    Hidden = entry.Hidden,
                    Collective = entry.Collective,
                    Costs = entry.Costs,
                    Constraints = entry.Constraints,
                    Modifiers = entry.Modifiers,
                    ModifierGroups = entry.ModifierGroups,
                    SelectionEntries = entry.SelectionEntries,
                    SelectionEntryGroups = entry.SelectionEntryGroups,
                    EntryLinks = entry.EntryLinks,
                    CategoryLinks = MergeCategoryLinks(entry.CategoryLinks, groupCatLinks),
                    Profiles = entry.Profiles,
                    Rules = entry.Rules,
                    InfoGroups = entry.InfoGroups,
                    InfoLinks = entry.InfoLinks,
                    Page = entry.Page,
                    PublicationId = entry.PublicationId,
                };
            }
            return new RosterSelection
            {
                Entry = effectiveEntry,
                SourceLink = avail.SourceLink,
                Number = 1
            };
        }

        if (avail.Group is { } group)
        {
            var groupEntry = new ProtocolSelectionEntry
            {
                Id = group.Id,
                Name = group.Name,
                Type = "upgrade",
                Hidden = group.Hidden,
                Collective = group.Collective,
                Costs = group.Costs,
                Constraints = group.Constraints,
                Modifiers = group.Modifiers,
                ModifierGroups = group.ModifierGroups,
                SelectionEntries = group.SelectionEntries,
                SelectionEntryGroups = group.SelectionEntryGroups,
                EntryLinks = group.EntryLinks,
                CategoryLinks = group.CategoryLinks,
                Profiles = group.Profiles,
                Rules = group.Rules,
                InfoGroups = group.InfoGroups,
                InfoLinks = group.InfoLinks,
                Page = group.Page,
                PublicationId = group.PublicationId,
            };

            return new RosterSelection
            {
                Entry = groupEntry,
                SourceLink = avail.SourceLink,
                SourceGroup = group,
                Number = 1
            };
        }

        throw new InvalidOperationException("AvailableEntry must have either Entry or Group");
    }

    private SelectionState BuildSelectionState(RosterSelection sel, RosterForce force, ModifierEvaluator evaluator,
        RosterSelection? parentSel = null)
    {
        var effectiveName = evaluator.GetEffectiveName(sel.Entry, sel, force, parentSel);
        var effectiveHidden = evaluator.GetEffectiveHidden(sel.Entry, sel, force);
        var effectiveCosts = evaluator.GetEffectiveCosts(sel.Entry, sel, force, parentSel);
        var effectivePage = evaluator.GetEffectivePage(sel.Entry, sel, force, parentSel);

        var children = new List<SelectionState>();
        foreach (var child in sel.Children)
        {
            children.Add(BuildSelectionState(child, force, evaluator, sel));
        }

        var costs = effectiveCosts.Select(c => new CostState(
            Name: c.Name,
            TypeId: c.TypeId,
            Value: c.Value * (sel.Entry.Collective ? 1 : sel.Number)
        )).ToList();

        var profiles = BuildProfiles(sel.Entry, evaluator, sel, force);
        var rules = BuildRules(sel.Entry, evaluator, sel, force);
        var categories = BuildCategories(sel.Entry, evaluator, sel, force);

        var publicationId = sel.Entry.PublicationId;
        var publicationName = ResolvePublicationName(publicationId);

        return new SelectionState(
            Name: effectiveName,
            EntryId: sel.Entry.Id,
            Type: sel.Entry.Type,
            Number: sel.Number,
            Hidden: effectiveHidden,
            Costs: costs,
            Children: children,
            Profiles: profiles.Count > 0 ? profiles : null,
            Rules: rules.Count > 0 ? rules : null,
            Categories: categories.Count > 0 ? categories : null,
            Page: effectivePage,
            PublicationId: publicationId,
            PublicationName: publicationName
        );
    }

    private List<ProfileState> BuildProfiles(ProtocolSelectionEntry entry, ModifierEvaluator evaluator,
        RosterSelection? selection, RosterForce? force)
    {
        var profiles = new List<ProfileState>();
        var allProfiles = _resolver.ResolveAllProfiles(entry);
        if (allProfiles.Count == 0) return profiles;

        foreach (var profile in allProfiles)
        {
            var characteristics = new List<CharacteristicState>();
            if (profile.Characteristics is { } chars)
            {
                foreach (var ch in chars)
                {
                    var value = ch.Value;

                    // Apply PROFILE-level modifiers
                    if (profile.Modifiers is { } profileMods)
                    {
                        foreach (var mod in profileMods)
                        {
                            if (mod.Field == ch.TypeId && evaluator.EvaluateConditions(mod, entry, selection, force))
                            {
                                var repeatCount = evaluator.GetRepeatCount(mod.Repeats, entry, selection, force);
                                if (repeatCount <= 0) continue;
                                value = ApplyCharacteristicModifier(mod.Type, value, mod.Value, repeatCount);
                            }
                        }
                    }

                    // Apply PROFILE-level modifier groups
                    if (profile.ModifierGroups is { } profileModGroups)
                    {
                        foreach (var group in profileModGroups)
                        {
                            if (!evaluator.EvaluateGroupConditions(group, entry, selection, force)) continue;
                            if (group.Modifiers is { } grpMods)
                            {
                                foreach (var mod in grpMods)
                                {
                                    if (mod.Field == ch.TypeId && evaluator.EvaluateConditions(mod, entry, selection, force))
                                    {
                                        var repeatCount = evaluator.GetRepeatCount(mod.Repeats, entry, selection, force);
                                        if (repeatCount <= 0) continue;
                                        value = ApplyCharacteristicModifier(mod.Type, value, mod.Value, repeatCount);
                                    }
                                }
                            }
                        }
                    }

                    characteristics.Add(new CharacteristicState(Name: ch.Name, TypeId: ch.TypeId, Value: value));
                }
            }

            profiles.Add(new ProfileState(
                Name: profile.Name, TypeId: profile.TypeId, TypeName: profile.TypeName,
                Hidden: profile.Hidden, Characteristics: characteristics,
                Page: profile.Page, PublicationId: profile.PublicationId));
        }

        return profiles;
    }

    private List<RuleState> BuildRules(ProtocolSelectionEntry entry, ModifierEvaluator evaluator,
        RosterSelection? selection, RosterForce? force)
    {
        var rules = new List<RuleState>();
        var allRules = _resolver.ResolveAllRules(entry);
        if (allRules.Count == 0) return rules;

        foreach (var rule in allRules)
        {
            var description = rule.Description ?? "";

            // Apply RULE-level modifiers
            if (rule.Modifiers is { } ruleMods)
            {
                foreach (var mod in ruleMods)
                {
                    if (mod.Field == "description" && evaluator.EvaluateConditions(mod, entry, selection, force))
                    {
                        var repeatCount = evaluator.GetRepeatCount(mod.Repeats, entry, selection, force);
                        if (repeatCount <= 0) continue;
                        description = ModifierEvaluator.ApplyStringModifierStatic(mod.Type, description, mod.Value, repeatCount);
                    }
                }
            }

            // Apply RULE-level modifier groups
            if (rule.ModifierGroups is { } ruleModGroups)
            {
                foreach (var group in ruleModGroups)
                {
                    if (!evaluator.EvaluateGroupConditions(group, entry, selection, force)) continue;
                    if (group.Modifiers is { } grpMods)
                    {
                        foreach (var mod in grpMods)
                        {
                            if (mod.Field == "description" && evaluator.EvaluateConditions(mod, entry, selection, force))
                            {
                                var repeatCount = evaluator.GetRepeatCount(mod.Repeats, entry, selection, force);
                                if (repeatCount <= 0) continue;
                                description = ModifierEvaluator.ApplyStringModifierStatic(mod.Type, description, mod.Value, repeatCount);
                            }
                        }
                    }
                }
            }

            rules.Add(new RuleState(
                Name: rule.Name, Description: description, Hidden: rule.Hidden,
                Page: rule.Page, PublicationId: rule.PublicationId));
        }

        return rules;
    }

    private List<ProfileState> BuildForceProfiles(ProtocolForceEntry forceEntry)
    {
        var result = new List<ProfileState>();
        var allProfiles = _resolver.ResolveForceEntryProfiles(forceEntry);

        foreach (var profile in allProfiles)
        {
            var characteristics = new List<CharacteristicState>();
            if (profile.Characteristics is { } chars)
            {
                foreach (var ch in chars)
                    characteristics.Add(new CharacteristicState(Name: ch.Name, TypeId: ch.TypeId, Value: ch.Value));
            }

            result.Add(new ProfileState(
                Name: profile.Name, TypeId: profile.TypeId, TypeName: profile.TypeName,
                Hidden: profile.Hidden, Characteristics: characteristics,
                Page: profile.Page, PublicationId: profile.PublicationId));
        }

        return result;
    }

    private List<RuleState> BuildForceRules(ProtocolForceEntry forceEntry)
    {
        var result = new List<RuleState>();
        var allRules = _resolver.ResolveForceEntryRules(forceEntry);

        foreach (var rule in allRules)
        {
            result.Add(new RuleState(
                Name: rule.Name, Description: rule.Description ?? "",
                Hidden: rule.Hidden, Page: rule.Page, PublicationId: rule.PublicationId));
        }

        return result;
    }

    private List<CategoryState> BuildCategories(ProtocolSelectionEntry entry, ModifierEvaluator evaluator,
        RosterSelection? selection, RosterForce? force)
    {
        var categories = new List<CategoryState>();

        // Include categoryLinks from entry itself
        if (entry.CategoryLinks is { } links)
        {
            foreach (var link in links)
            {
                var primary = link.Primary;
                primary = EvaluateCategoryLinkPrimary(link, primary, entry, evaluator, selection, force);
                categories.Add(new CategoryState(Name: link.Name, EntryId: link.TargetId, Primary: primary));
            }
        }

        // Inherit categoryLinks from the source group (selectionEntryGroup)
        if (selection?.SourceGroup?.CategoryLinks is { } groupCatLinks)
        {
            var existingIds = new HashSet<string>(categories.Select(c => c.EntryId), StringComparer.Ordinal);
            foreach (var gcl in groupCatLinks)
            {
                if (!existingIds.Contains(gcl.TargetId))
                    categories.Add(new CategoryState(Name: gcl.Name, EntryId: gcl.TargetId, Primary: gcl.Primary));
            }
        }

        // Apply entry-level category modifiers (set-primary, unset-primary)
        ApplyCategoryModifiers(entry.Modifiers, entry.ModifierGroups, categories, entry, evaluator, selection, force);

        return categories;
    }

    private bool EvaluateCategoryLinkPrimary(ProtocolCategoryLink link, bool initial,
        ProtocolSelectionEntry entry, ModifierEvaluator evaluator,
        RosterSelection? selection, RosterForce? force)
    {
        var primary = initial;

        if (link.Modifiers is { } mods)
        {
            foreach (var mod in mods)
            {
                if (mod.Field != "primary") continue;
                if (!evaluator.EvaluateConditions(mod, entry, selection, force)) continue;
                if (mod.Type == "set")
                    primary = string.Equals(mod.Value, "true", StringComparison.OrdinalIgnoreCase);
            }
        }

        if (link.ModifierGroups is { } groups)
        {
            foreach (var group in groups)
            {
                if (!evaluator.EvaluateGroupConditions(group, entry, selection, force)) continue;
                if (group.Modifiers is { } grpMods)
                {
                    foreach (var mod in grpMods)
                    {
                        if (mod.Field != "primary") continue;
                        if (!evaluator.EvaluateConditions(mod, entry, selection, force)) continue;
                        if (mod.Type == "set")
                            primary = string.Equals(mod.Value, "true", StringComparison.OrdinalIgnoreCase);
                    }
                }
            }
        }

        return primary;
    }

    private void ApplyCategoryModifiers(List<ProtocolModifier>? modifiers, List<ProtocolModifierGroup>? groups,
        List<CategoryState> categories, ProtocolSelectionEntry entry, ModifierEvaluator evaluator,
        RosterSelection? selection, RosterForce? force)
    {
        if (modifiers is { } mods)
        {
            foreach (var mod in mods)
            {
                if (mod.Field != "category") continue;
                if (!evaluator.EvaluateConditions(mod, entry, selection, force)) continue;

                if (mod.Type == "set-primary")
                {
                    // If the target category is not present, add it
                    bool found = false;
                    for (int i = 0; i < categories.Count; i++)
                    {
                        if (categories[i].EntryId == mod.Value) found = true;
                        categories[i] = categories[i] with { Primary = categories[i].EntryId == mod.Value };
                    }
                    if (!found)
                    {
                        var catName = ResolveCategoryName(mod.Value);
                        categories.Add(new CategoryState(Name: catName, EntryId: mod.Value, Primary: true));
                    }
                }
                else if (mod.Type == "unset-primary")
                {
                    for (int i = 0; i < categories.Count; i++)
                    {
                        if (categories[i].EntryId == mod.Value)
                            categories[i] = categories[i] with { Primary = false };
                    }
                }
            }
        }

        if (groups is { } grps)
        {
            foreach (var group in grps)
            {
                if (!evaluator.EvaluateGroupConditions(group, entry, selection, force)) continue;
                ApplyCategoryModifiers(group.Modifiers, group.ModifierGroups, categories, entry, evaluator, selection, force);
            }
        }
    }

    private static string ApplyCharacteristicModifier(string type, string current, string value, int repeatCount)
    {
        if (repeatCount <= 0) repeatCount = 1;

        return type switch
        {
            "set" => value,
            "append" => AppendRepeated(current, value, repeatCount),
            "increment" when double.TryParse(current, System.Globalization.NumberStyles.Any,
                    System.Globalization.CultureInfo.InvariantCulture, out var curNum)
                && double.TryParse(value, System.Globalization.NumberStyles.Any,
                    System.Globalization.CultureInfo.InvariantCulture, out var incVal)
                => FormatNumber(curNum + incVal * repeatCount),
            "decrement" when double.TryParse(current, System.Globalization.NumberStyles.Any,
                    System.Globalization.CultureInfo.InvariantCulture, out var curNum2)
                && double.TryParse(value, System.Globalization.NumberStyles.Any,
                    System.Globalization.CultureInfo.InvariantCulture, out var decVal)
                => FormatNumber(curNum2 - decVal * repeatCount),
            _ => current
        };

        static string AppendRepeated(string current, string value, int count)
        {
            for (int i = 0; i < count; i++)
                current = current + " " + value;
            return current;
        }
    }

    internal static string FormatNumber(double value)
    {
        if (value == Math.Floor(value) && !double.IsInfinity(value))
            return ((long)value).ToString();
        return value.ToString(System.Globalization.CultureInfo.InvariantCulture);
    }

    private List<CostState> AggregateTotalCosts(ModifierEvaluator evaluator)
    {
        var totals = new Dictionary<string, double>(StringComparer.Ordinal);
        var referencedTypes = new HashSet<string>(StringComparer.Ordinal);

        // Track cost types referenced by any entry (root or child, direct or shared)
        CollectReferencedCostTypes(referencedTypes);

        foreach (var force in _forces)
        {
            foreach (var sel in force.Selections)
                AggregateCostsRecursive(sel, totals, force, evaluator);
        }

        // Only include cost types that are referenced by available entries
        var result = new List<CostState>();
        if (_gameSystem.CostTypes is { } costTypes)
        {
            foreach (var ct in costTypes)
            {
                if (referencedTypes.Contains(ct.Id))
                {
                    result.Add(new CostState(
                        Name: ct.Name,
                        TypeId: ct.Id,
                        Value: totals.GetValueOrDefault(ct.Id, 0)
                    ));
                }
            }
        }

        return result;
    }

    private void CollectReferencedCostTypes(HashSet<string> referencedTypes)
    {
        // Scan all available entries and their children
        foreach (var force in _forces)
        {
            var available = _resolver.GetAvailableEntries(force.Catalogue);
            foreach (var avail in available)
            {
                if (avail.Entry is { } entry)
                    CollectCostTypesRecursive(entry, referencedTypes);
            }
        }
    }

    private static void CollectCostTypesRecursive(ProtocolSelectionEntry entry, HashSet<string> types)
    {
        if (entry.Costs is { } costs)
            foreach (var c in costs)
                types.Add(c.TypeId);
        if (entry.SelectionEntries is { } children)
            foreach (var child in children)
                CollectCostTypesRecursive(child, types);
    }

    private static void AggregateCostsRecursive(RosterSelection sel, Dictionary<string, double> totals,
        RosterForce force, ModifierEvaluator evaluator)
    {
        var costs = evaluator.GetEffectiveCosts(sel.Entry, sel, force);
        foreach (var cost in costs)
        {
            totals.TryGetValue(cost.TypeId, out var current);
            totals[cost.TypeId] = current + cost.Value * (sel.Entry.Collective ? 1 : sel.Number);
        }

        foreach (var child in sel.Children)
            AggregateCostsRecursive(child, totals, force, evaluator);
    }

    private string? ResolvePublicationName(string? publicationId)
    {
        if (publicationId is null) return null;

        if (_gameSystem.Publications is { } gsPubs)
        {
            var pub = gsPubs.FirstOrDefault(p => p.Id == publicationId);
            if (pub is not null) return pub.Name;
        }

        foreach (var cat in _catalogues)
        {
            if (cat.Publications is { } catPubs)
            {
                var pub = catPubs.FirstOrDefault(p => p.Id == publicationId);
                if (pub is not null) return pub.Name;
            }
        }

        return null;
    }

    private string ResolveCategoryName(string categoryId)
    {
        if (_gameSystem.CategoryEntries is { } cats)
        {
            var cat = cats.FirstOrDefault(c => c.Id == categoryId);
            if (cat is not null) return cat.Name;
        }
        return categoryId;
    }

    private static void CopyChildren(List<RosterSelection> source, List<RosterSelection> target)
    {
        foreach (var child in source)
        {
            var copy = new RosterSelection
            {
                Entry = child.Entry,
                SourceLink = child.SourceLink,
                SourceGroup = child.SourceGroup,
                Number = child.Number
            };
            CopyChildren(child.Children, copy.Children);
            target.Add(copy);
        }
    }

    private static List<ProtocolCategoryLink>? MergeCategoryLinks(
        List<ProtocolCategoryLink>? first, List<ProtocolCategoryLink> second)
    {
        if (first is null or { Count: 0 }) return second;
        return [.. first, .. second];
    }

    public void Dispose()
    {
        _forces.Clear();
        _costLimits.Clear();
    }
}
