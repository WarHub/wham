using BattleScribeSpec;
using BattleScribeSpec.Protocol;

namespace WarHub.ArmouryModel.RosterEngine;

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

        // Initialize cost limits from cost types
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
        var forceEntry = _gameSystem.ForceEntries![forceEntryIndex];
        var catalogue = _catalogues[catalogueIndex];

        var force = new RosterForce
        {
            ForceEntry = forceEntry,
            Catalogue = catalogue
        };

        _forces.Add(force);

        // Auto-select entries with min constraints
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
        var force = _forces[forceIndex];
        force.Selections[entryIndex].Number = count;
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

        // Deep copy children
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

            forces.Add(new ForceState(
                Name: force.ForceEntry.Name,
                CatalogueId: force.Catalogue.Id,
                Selections: selections,
                AvailableEntryCount: _resolver.GetAvailableEntries(force.Catalogue).Count,
                PublicationId: force.ForceEntry.PublicationId,
                Page: force.ForceEntry.Page
            ));
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

    // ===== Private helpers =====

    private ModifierEvaluator CreateEvaluator() => new(_gameSystem, _forces);

    private void AutoSelectEntries(RosterForce force)
    {
        var available = _resolver.GetAvailableEntries(force.Catalogue);
        var evaluator = CreateEvaluator();

        foreach (var avail in available)
        {
            if (avail.Entry is not { } entry) continue;

            // Check if entry has a min constraint with field=selections and scope=parent
            if (entry.Constraints is not { } constraints) continue;

            foreach (var constraint in constraints)
            {
                if (constraint.Type != "min" || constraint.Field != "selections" || constraint.Scope != "parent")
                    continue;

                var effectiveValue = evaluator.GetEffectiveConstraintValue(constraint, entry, null, force);
                if (effectiveValue < 1) continue;
                if (constraint.PercentValue) continue;

                // Check if entry is hidden (don't auto-select hidden entries)
                var hidden = evaluator.GetEffectiveHidden(entry, null, force);
                if (hidden) continue;

                // Auto-select
                var selection = CreateSelection(avail);
                force.Selections.Add(selection);
                break; // Only auto-select once per entry
            }
        }
    }

    private static RosterSelection CreateSelection(AvailableEntry avail)
    {
        if (avail.Entry is { } entry)
        {
            return new RosterSelection
            {
                Entry = entry,
                SourceLink = avail.SourceLink,
                Number = 1
            };
        }

        if (avail.Group is { } group)
        {
            // For groups, create a placeholder selection
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

    private SelectionState BuildSelectionState(RosterSelection sel, RosterForce force, ModifierEvaluator evaluator)
    {
        var effectiveName = evaluator.GetEffectiveName(sel.Entry, sel, force);
        var effectiveHidden = evaluator.GetEffectiveHidden(sel.Entry, sel, force);
        var effectiveCosts = evaluator.GetEffectiveCosts(sel.Entry, sel, force);

        var children = new List<SelectionState>();
        foreach (var child in sel.Children)
        {
            children.Add(BuildSelectionState(child, force, evaluator));
        }

        var costs = effectiveCosts.Select(c => new CostState(
            Name: c.Name,
            TypeId: c.TypeId,
            Value: c.Value * (sel.Entry.Collective ? 1 : sel.Number)
        )).ToList();

        // Build profiles
        var profiles = BuildProfiles(sel.Entry, evaluator, sel, force);

        // Build rules
        var rules = BuildRules(sel.Entry, evaluator, sel, force);

        // Build categories
        var categories = BuildCategories(sel.Entry);

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
            Page: sel.Entry.Page,
            PublicationId: sel.Entry.PublicationId
        );
    }

    private static List<ProfileState> BuildProfiles(ProtocolSelectionEntry entry, ModifierEvaluator evaluator,
        RosterSelection? selection, RosterForce? force)
    {
        var profiles = new List<ProfileState>();
        if (entry.Profiles is not { } entryProfiles) return profiles;

        foreach (var profile in entryProfiles)
        {
            var characteristics = new List<CharacteristicState>();
            if (profile.Characteristics is { } chars)
            {
                foreach (var ch in chars)
                {
                    // Apply modifiers to characteristic values
                    var value = ch.Value;
                    if (entry.Modifiers is { } mods)
                    {
                        foreach (var mod in mods)
                        {
                            if (mod.Field == ch.TypeId && evaluator.EvaluateConditions(mod, entry, selection, force))
                            {
                                var repeatCount = evaluator.GetRepeatCount(mod.Repeats, entry, selection, force);
                                value = mod.Type switch
                                {
                                    "set" => mod.Value,
                                    "append" => value + " " + mod.Value,
                                    _ => value
                                };
                            }
                        }
                    }

                    characteristics.Add(new CharacteristicState(
                        Name: ch.Name,
                        TypeId: ch.TypeId,
                        Value: value
                    ));
                }
            }

            profiles.Add(new ProfileState(
                Name: profile.Name,
                TypeId: profile.TypeId,
                TypeName: profile.TypeName,
                Hidden: profile.Hidden,
                Characteristics: characteristics,
                Page: profile.Page,
                PublicationId: profile.PublicationId
            ));
        }

        return profiles;
    }

    private static List<RuleState> BuildRules(ProtocolSelectionEntry entry, ModifierEvaluator evaluator,
        RosterSelection? selection, RosterForce? force)
    {
        var rules = new List<RuleState>();
        if (entry.Rules is not { } entryRules) return rules;

        foreach (var rule in entryRules)
        {
            var description = rule.Description ?? "";

            // Apply modifiers to description
            if (entry.Modifiers is { } mods)
            {
                foreach (var mod in mods)
                {
                    if (mod.Field == "description" && evaluator.EvaluateConditions(mod, entry, selection, force))
                    {
                        description = mod.Type switch
                        {
                            "set" => mod.Value,
                            "append" => description + " " + mod.Value,
                            _ => description
                        };
                    }
                }
            }

            rules.Add(new RuleState(
                Name: rule.Name,
                Description: description,
                Hidden: rule.Hidden,
                Page: rule.Page,
                PublicationId: rule.PublicationId
            ));
        }

        return rules;
    }

    private static List<CategoryState> BuildCategories(ProtocolSelectionEntry entry)
    {
        var categories = new List<CategoryState>();
        if (entry.CategoryLinks is not { } links) return categories;

        foreach (var link in links)
        {
            categories.Add(new CategoryState(
                Name: link.Name,
                EntryId: link.TargetId,
                Primary: link.Primary
            ));
        }

        return categories;
    }

    private List<CostState> AggregateTotalCosts(ModifierEvaluator evaluator)
    {
        var totals = new Dictionary<string, double>(StringComparer.Ordinal);

        foreach (var force in _forces)
        {
            foreach (var sel in force.Selections)
            {
                AggregateCostsRecursive(sel, totals, force, evaluator);
            }
        }

        return totals.Select(kvp => new CostState(
            Name: _gameSystem.CostTypes?.FirstOrDefault(ct => ct.Id == kvp.Key)?.Name ?? kvp.Key,
            TypeId: kvp.Key,
            Value: kvp.Value
        )).ToList();
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
        {
            AggregateCostsRecursive(child, totals, force, evaluator);
        }
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

    public void Dispose()
    {
        _forces.Clear();
        _costLimits.Clear();
    }
}
