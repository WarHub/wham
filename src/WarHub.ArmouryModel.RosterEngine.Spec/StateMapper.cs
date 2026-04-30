using BattleScribeSpec;
using BattleScribeSpec.Protocol;
using WarHub.ArmouryModel.Concrete;
using WarHub.ArmouryModel.RosterEngine;
using ProtocolRosterState = BattleScribeSpec.RosterState;

namespace WarHub.ArmouryModel.RosterEngine.Spec;

/// <summary>
/// Maps the ISymbol-based roster tree to BattleScribeSpec Protocol types.
/// Thin mapping layer — reads the public Symbol API surface and delegates
/// ordering to <see cref="SelectionOrdering"/>.
/// </summary>
internal sealed class StateMapper
{
    private readonly IRosterSymbol _roster;
    private readonly WhamCompilation _compilation;

    public StateMapper(IRosterSymbol roster, WhamCompilation compilation)
    {
        _roster = roster;
        _compilation = compilation;
    }

    public ProtocolRosterState MapRosterState()
    {
        // Collect referenced cost types from all force catalogues' available entries
        var referencedCostTypeIds = new HashSet<string>(StringComparer.Ordinal);
        foreach (var forceSymbol in _roster.Forces)
        {
            var catalogue = forceSymbol.CatalogueReference.Catalogue;
            var entries = EntryResolver.GetAvailableEntries(catalogue);
            foreach (var entry in entries)
            {
                CollectReferencedCostTypes(entry.Symbol, referencedCostTypeIds);
            }
        }

        // Build cost type ID → name map for filling missing costs on selections
        var costTypeNames = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var rosterCost in _roster.Costs)
        {
            var typeId = rosterCost.CostType?.Id ?? "";
            if (referencedCostTypeIds.Contains(typeId))
                costTypeNames.TryAdd(typeId, rosterCost.Name ?? "");
        }

        var forces = new List<ForceState>(_roster.Forces.Length);
        for (int i = 0; i < _roster.Forces.Length; i++)
        {
            forces.Add(MapForce(_roster.Forces[i], costTypeNames));
        }
        // Sort forces alphabetically by name (BattleScribe canonical ordering)
        forces.Sort((a, b) => SelectionOrdering.NaturalSort.Compare(a.Name, b.Name));

        var costs = ComputeRosterCosts(forces, referencedCostTypeIds);
        var errors = GetConstraintErrors();
        var costLimits = MapCostLimits();

        return new ProtocolRosterState(
            Name: _roster.Name ?? "New Roster",
            GameSystemId: _roster.ContainingNamespace?.RootCatalogue?.Id ?? "",
            Forces: forces,
            Costs: costs,
            ValidationErrors: errors,
            CostLimits: costLimits.Count > 0 ? costLimits : null,
            GameSystemName: _roster.ContainingNamespace?.RootCatalogue?.Name);
    }

    private ForceState MapForce(IForceSymbol force, IReadOnlyDictionary<string, string> costTypeNames)
    {
        var catalogue = force.CatalogueReference.Catalogue;
        var availableEntryCount = EntryResolver.GetRootEntryCount(catalogue);

        var selections = new List<SelectionState>(force.Selections.Length);
        foreach (var sel in SelectionOrdering.GetSortedSelections(force))
        {
            selections.Add(MapSelection(sel, costTypeNames));
        }

        var profiles = MapProfiles(force.EffectiveSourceEntry.Resources);
        var rules = MapRules(force.EffectiveSourceEntry.Resources);

        // Collect force-level rules from catalogue and game system root resource entries
        var gamesystem = catalogue.Gamesystem;
        AppendRootRules(catalogue, rules);
        if (!catalogue.IsGamesystem)
            AppendRootRules(gamesystem, rules);

        var categories = MapForceCategories(force);
        var publications = MapPublications(force);
        var childForces = MapChildForces(force, costTypeNames);

        return new ForceState(
            Id: force.Id,
            Name: force.Name,
            CatalogueId: force.CatalogueReference.Catalogue.Id,
            Selections: selections,
            AvailableEntryCount: availableEntryCount,
            Hidden: force.EffectiveSourceEntry.IsHidden,
            PublicationId: force.PublicationReference?.PublicationId,
            Page: force.Page,
            EntryId: force.EntryId,
            CatalogueName: force.CatalogueReference.Catalogue.Name,
            CustomName: force.CustomName,
            CustomNotes: force.CustomNotes)
        {
            Profiles = profiles.Count > 0 ? profiles : [],
            Rules = rules.Count > 0 ? rules : [],
            Categories = categories.Count > 0 ? categories : [],
            Publications = publications.Count > 0 ? publications : [],
            ChildForces = childForces.Count > 0 ? childForces : [],
        };
    }

    private SelectionState MapSelection(ISelectionSymbol sel, IReadOnlyDictionary<string, string> costTypeNames)
    {
        var children = new List<SelectionState>(sel.Selections.Length);
        foreach (var child in SelectionOrdering.GetSortedChildSelections(sel))
        {
            children.Add(MapSelection(child, costTypeNames));
        }

        var eff = sel.EffectiveSourceEntry;

        var costs = MapSelectionCosts(eff, sel.SelectedCount, costTypeNames);
        var categories = MapSelectionCategories(eff);
        var profiles = MapProfiles(eff.Resources);
        var rules = MapRules(eff.Resources);
        var page = eff.Page;

        var type = sel.EntryKind switch
        {
            Source.SelectionEntryKind.Unit => "unit",
            Source.SelectionEntryKind.Model => "model",
            Source.SelectionEntryKind.Upgrade => "upgrade",
            _ => "upgrade",
        };

        return new SelectionState(
            Id: sel.Id,
            Name: eff.Name,
            EntryId: sel.EntryId,
            Type: type,
            Number: sel.SelectedCount,
            Hidden: eff.IsHidden,
            Costs: costs,
            Children: children,
            Profiles: profiles.Count > 0 ? profiles : null,
            Rules: rules.Count > 0 ? rules : null,
            Categories: categories.Count > 0 ? categories : null,
            Page: page,
            PublicationId: sel.PublicationReference?.PublicationId,
            PublicationName: sel.PublicationReference?.Publication?.Name,
            EntryGroupId: sel.EntryGroupId,
            CustomName: sel.CustomName,
            CustomNotes: sel.CustomNotes);
    }

    // ──────────────────────────────────────────────────────────────────
    //  Profile / Rule mapping from effective symbols
    // ──────────────────────────────────────────────────────────────────

    private static List<ProfileState> MapProfiles(ImmutableArray<IResourceEntrySymbol> resources)
    {
        var result = new List<ProfileState>(resources.Length);
        foreach (var resource in resources)
        {
            if (resource is not IProfileSymbol p)
                continue;
            if (p.IsHidden)
                continue;
            var chars = new List<CharacteristicState>(p.Characteristics.Length);
            foreach (var ch in p.Characteristics)
            {
                chars.Add(new CharacteristicState(
                    Name: ch.Name ?? "",
                    TypeId: ch.Type?.Id ?? "",
                    Value: ch.Value));
            }
            result.Add(new ProfileState(
                Name: p.Name ?? "",
                TypeId: p.Type?.Id,
                TypeName: p.Type?.Name,
                Hidden: p.IsHidden,
                Characteristics: chars,
                Page: p.Page,
                PublicationId: p.PublicationReference?.PublicationId));
        }
        return result;
    }

    private static List<RuleState> MapRules(ImmutableArray<IResourceEntrySymbol> resources)
    {
        var result = new List<RuleState>(resources.Length);
        foreach (var resource in resources)
        {
            if (resource is IRuleSymbol { IsHidden: false } r)
            {
                result.Add(new RuleState(
                    Name: r.Name ?? "",
                    Description: r.DescriptionText,
                    Hidden: false,
                    Page: r.Page,
                    PublicationId: r.PublicationReference?.PublicationId));
            }
        }
        return result;
    }

    private static void AppendRootRules(ICatalogueSymbol catalogue, List<RuleState> rules)
    {
        foreach (var resource in catalogue.RootResourceEntries)
        {
            if (resource is IRuleSymbol { IsHidden: false } rule)
            {
                rules.Add(new RuleState(
                    Name: rule.Name ?? "",
                    Description: rule.DescriptionText,
                    Hidden: false,
                    Page: rule.Page,
                    PublicationId: rule.PublicationReference?.PublicationId));
            }
        }
    }

    // ──────────────────────────────────────────────────────────────────
    //  Category mapping
    // ──────────────────────────────────────────────────────────────────

    private static List<CategoryState> MapSelectionCategories(ISelectionEntryContainerSymbol eff)
    {
        var categories = new List<CategoryState>(eff.Categories.Length);
        foreach (var cat in eff.Categories)
        {
            var entryId = cat.ReferencedEntry?.Id ?? cat.Id ?? "";
            categories.Add(new CategoryState(
                Name: cat.Name ?? "",
                EntryId: entryId,
                Primary: cat == eff.PrimaryCategory,
                PublicationId: cat.PublicationReference?.PublicationId,
                Page: cat.Page));
        }
        return categories;
    }

    private static List<CategoryState> MapForceCategories(IForceSymbol force)
    {
        var categories = new List<CategoryState>(force.Categories.Length + 1);
        // Prepend synthetic "Uncategorised" category (BattleScribe convention)
        categories.Add(new CategoryState(
            Name: "Uncategorised",
            EntryId: "(No Category)",
            Primary: false,
            PublicationId: null,
            Page: null));
        foreach (var cat in force.Categories)
        {
            var entryId = cat.SourceEntry?.Id ?? cat.Id ?? "";
            categories.Add(new CategoryState(
                Name: cat.Name ?? "",
                EntryId: entryId,
                Primary: cat.IsPrimaryCategory,
                PublicationId: cat.PublicationReference?.PublicationId,
                Page: cat.Page,
                CustomNotes: cat.CustomNotes));
        }
        return categories;
    }

    // ──────────────────────────────────────────────────────────────────
    //  Publication mapping
    // ──────────────────────────────────────────────────────────────────

    private List<PublicationState> MapPublications(IForceSymbol force)
    {
        var catalogue = force.CatalogueReference.Catalogue;
        var gamesystem = catalogue.Gamesystem;
        var result = new List<PublicationState>(catalogue.ResourceDefinitions.Length + gamesystem.ResourceDefinitions.Length);
        var seen = new HashSet<string>(StringComparer.Ordinal);
        if (!catalogue.IsGamesystem)
        {
            foreach (var def in catalogue.ResourceDefinitions)
            {
                if (def is IPublicationSymbol pub && pub.Id is not null && seen.Add(pub.Id))
                    result.Add(new PublicationState(Id: pub.Id, Name: pub.Name ?? ""));
            }
        }

        // Then game system publications
        foreach (var def in gamesystem.ResourceDefinitions)
        {
            if (def is IPublicationSymbol pub && pub.Id is not null && seen.Add(pub.Id))
                result.Add(new PublicationState(Id: pub.Id, Name: pub.Name ?? ""));
        }

        return result;
    }

    // ──────────────────────────────────────────────────────────────────
    //  Child force mapping
    // ──────────────────────────────────────────────────────────────────

    private List<ForceState> MapChildForces(IForceSymbol force, IReadOnlyDictionary<string, string> costTypeNames)
    {
        var childForces = new List<ForceState>(force.Forces.Length);
        foreach (var child in force.Forces)
        {
            childForces.Add(MapForce(child, costTypeNames));
        }
        return childForces;
    }

    // ──────────────────────────────────────────────────────────────────
    //  Cost mapping
    // ──────────────────────────────────────────────────────────────────

    private static List<CostState> MapSelectionCosts(
        ISelectionEntryContainerSymbol eff, int selectedCount,
        IReadOnlyDictionary<string, string> costTypeNames)
    {
        var costs = new List<CostState>(eff.Costs.Length);
        var emittedTypeIds = new HashSet<string>(StringComparer.Ordinal);
        foreach (var cost in eff.Costs)
        {
            var typeId = cost.Type?.Id ?? "";
            emittedTypeIds.Add(typeId);
            costs.Add(new CostState(
                Name: cost.Name ?? "",
                TypeId: typeId,
                Value: (double)(cost.Value * selectedCount)));
        }
        // BattleScribe emits all referenced cost types on every selection, filling 0 for missing ones
        foreach (var (typeId, name) in costTypeNames)
        {
            if (!emittedTypeIds.Contains(typeId))
            {
                costs.Add(new CostState(
                    Name: name,
                    TypeId: typeId,
                    Value: 0));
            }
        }
        return costs;
    }

    private List<CostState> ComputeRosterCosts(
        List<ForceState> mappedForces,
        IReadOnlySet<string> referencedCostTypeIds)
    {
        var totals = new Dictionary<string, double>(StringComparer.Ordinal);
        foreach (var force in mappedForces)
        {
            AggregateCostsFromSelections(force.Selections, totals);
        }

        var result = new List<CostState>();
        foreach (var rosterCost in _roster.Costs)
        {
            var typeId = rosterCost.CostType?.Id ?? "";
            if (referencedCostTypeIds.Contains(typeId))
            {
                result.Add(new CostState(
                    Name: rosterCost.Name ?? "",
                    TypeId: typeId,
                    Value: totals.GetValueOrDefault(typeId, 0)));
            }
        }
        return result;
    }

    private List<CostState> MapCostLimits()
    {
        var result = new List<CostState>();
        foreach (var rosterCost in _roster.Costs)
        {
            if (rosterCost.Limit is { } limit && limit >= 0)
            {
                result.Add(new CostState(
                    Name: rosterCost.Name ?? "",
                    TypeId: rosterCost.CostType?.Id ?? "",
                    Value: (double)limit));
            }
        }
        return result;
    }

    private static void AggregateCostsFromSelections(
        IReadOnlyList<SelectionState> selections,
        Dictionary<string, double> totals)
    {
        foreach (var sel in selections)
        {
            foreach (var cost in sel.Costs)
            {
                totals.TryGetValue(cost.TypeId, out var current);
                totals[cost.TypeId] = current + cost.Value;
            }
            if (sel.Children is { Count: > 0 })
            {
                AggregateCostsFromSelections(sel.Children, totals);
            }
        }
    }

    // ──────────────────────────────────────────────────────────────────
    //  Constraint errors
    // ──────────────────────────────────────────────────────────────────

    private List<ValidationErrorState> GetConstraintErrors()
    {
        var diagnostics = _compilation.GetConstraintDiagnostics();
        var result = new List<ValidationErrorState>(diagnostics.Length);
        foreach (var diagnostic in diagnostics)
        {
            if (diagnostic is WhamDiagnostic whamDiag && whamDiag.DiagnosticInfo is WhamDiagnosticInfo info)
            {
                var args = info.Args;
                var ownerType = args.Length > 0 ? args[0] as string ?? "" : "";
                var ownerEntryId = args.Length > 1 ? args[1] as string : null;
                if (ownerEntryId is "") ownerEntryId = null;
                var entryId = args.Length > 2 ? args[2] as string ?? "" : "";
                var constraintId = args.Length > 3 ? args[3] as string : null;
                if (constraintId is "") constraintId = null;
                result.Add(new ValidationErrorState(
                    Message: diagnostic.GetMessage(),
                    OwnerType: ownerType,
                    OwnerEntryId: ownerEntryId,
                    EntryId: entryId,
                    ConstraintId: constraintId));
            }
        }
        return result;
    }

    /// <summary>
    /// Collects cost type IDs referenced by an entry and its children (recursive).
    /// </summary>
    internal static void CollectReferencedCostTypes(
        ISelectionEntryContainerSymbol symbol,
        HashSet<string> types)
    {
        foreach (var cost in symbol.Costs)
        {
            if (cost.Type?.Id is { } typeId)
                types.Add(typeId);
        }
        foreach (var child in symbol.ChildSelectionEntries)
        {
            CollectReferencedCostTypes(child, types);
        }
    }
}
