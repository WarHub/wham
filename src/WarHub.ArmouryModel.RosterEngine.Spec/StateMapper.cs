using BattleScribeSpec;
using BattleScribeSpec.Protocol;
using WarHub.ArmouryModel.Concrete;
using ProtocolRosterState = BattleScribeSpec.RosterState;

namespace WarHub.ArmouryModel.RosterEngine.Spec;

/// <summary>
/// Maps the ISymbol-based roster tree to BattleScribeSpec Protocol types.
/// Reads only the public Symbol API surface — no SourceNode access, no internal helpers.
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

    public ProtocolRosterState MapRosterState(
        IReadOnlyList<int> forceAvailableEntryCounts,
        IReadOnlySet<string> referencedCostTypeIds)
    {
        var forces = new List<ForceState>();
        for (int i = 0; i < _roster.Forces.Length; i++)
        {
            var count = i < forceAvailableEntryCounts.Count ? forceAvailableEntryCounts[i] : 0;
            forces.Add(MapForce(_roster.Forces[i], count));
        }

        var costs = ComputeRosterCosts(forces, referencedCostTypeIds);
        var errors = GetConstraintErrors();

        return new ProtocolRosterState(
            Name: _roster.Name ?? "New Roster",
            GameSystemId: _roster.ContainingNamespace?.RootCatalogue?.Id ?? "",
            Forces: forces,
            Costs: costs,
            ValidationErrors: errors);
    }

    private ForceState MapForce(IForceSymbol force, int availableEntryCount)
    {
        var selections = new List<SelectionState>();
        foreach (var sel in force.Selections)
        {
            selections.Add(MapSelection(sel));
        }

        var profiles = MapProfiles(force.EffectiveProfiles);
        var rules = MapRules(force.EffectiveRules);

        return new ForceState(
            Name: force.Name,
            CatalogueId: force.CatalogueReference.Catalogue.Id,
            Selections: selections,
            AvailableEntryCount: availableEntryCount,
            PublicationId: force.PublicationReference?.Publication?.Id,
            Page: force.Page)
        {
            Profiles = profiles.Count > 0 ? profiles : [],
            Rules = rules.Count > 0 ? rules : [],
        };
    }

    private SelectionState MapSelection(ISelectionSymbol sel)
    {
        var children = new List<SelectionState>();
        foreach (var child in sel.Selections)
        {
            children.Add(MapSelection(child));
        }

        var eff = sel.EffectiveSourceEntry;

        // Costs: use effective (modifier-applied) per-unit costs × SelectedCount
        var costs = MapSelectionCosts(eff, sel.SelectedCount);

        // Categories from effective entry (modifier-applied, includes group-inherited)
        var categories = MapCategories(eff);

        // Profiles and rules from effective entry
        var profiles = MapProfiles(eff.EffectiveProfiles);
        var rules = MapRules(eff.EffectiveRules);

        // Page: effective page (includes entry's declared page as fallback)
        var page = eff.EffectivePage ?? sel.Page;

        var type = sel.EntryKind switch
        {
            Source.SelectionEntryKind.Unit => "unit",
            Source.SelectionEntryKind.Model => "model",
            Source.SelectionEntryKind.Upgrade => "upgrade",
            _ => "upgrade",
        };

        return new SelectionState(
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
            PublicationId: sel.PublicationReference?.Publication?.Id,
            PublicationName: sel.PublicationReference?.Publication?.Name);
    }

    // ──────────────────────────────────────────────────────────────────
    //  Profile / Rule mapping from effective symbols
    // ──────────────────────────────────────────────────────────────────

    private static List<ProfileState> MapProfiles(ImmutableArray<IEffectiveProfileSymbol> effectiveProfiles)
    {
        var result = new List<ProfileState>(effectiveProfiles.Length);
        foreach (var p in effectiveProfiles)
        {
            var chars = new List<CharacteristicState>(p.Characteristics.Length);
            foreach (var ch in p.Characteristics)
            {
                chars.Add(new CharacteristicState(
                    Name: ch.Name,
                    TypeId: ch.TypeId,
                    Value: ch.Value));
            }
            result.Add(new ProfileState(
                Name: p.Name,
                TypeId: p.TypeId,
                TypeName: p.TypeName,
                Hidden: p.IsHidden,
                Characteristics: chars,
                Page: p.Page,
                PublicationId: p.PublicationId));
        }
        return result;
    }

    private static List<RuleState> MapRules(ImmutableArray<IEffectiveRuleSymbol> effectiveRules)
    {
        var result = new List<RuleState>(effectiveRules.Length);
        foreach (var r in effectiveRules)
        {
            result.Add(new RuleState(
                Name: r.Name,
                Description: r.Description,
                Hidden: r.IsHidden,
                Page: r.Page,
                PublicationId: r.PublicationId));
        }
        return result;
    }

    // ──────────────────────────────────────────────────────────────────
    //  Category mapping from effective entry
    // ──────────────────────────────────────────────────────────────────

    private static List<CategoryState> MapCategories(ISelectionEntryContainerSymbol eff)
    {
        var categories = new List<CategoryState>();
        foreach (var cat in eff.Categories)
        {
            var entryId = cat.ReferencedEntry?.Id ?? cat.Id ?? "";
            categories.Add(new CategoryState(
                Name: cat.Name ?? "",
                EntryId: entryId,
                Primary: cat == eff.PrimaryCategory));
        }
        return categories;
    }

    // ──────────────────────────────────────────────────────────────────
    //  Cost mapping
    // ──────────────────────────────────────────────────────────────────

    private static List<CostState> MapSelectionCosts(ISelectionEntryContainerSymbol eff, int selectedCount)
    {
        var costs = new List<CostState>();
        foreach (var cost in eff.Costs)
        {
            // Effective entry costs are per-unit (modifier-applied); multiply by count.
            costs.Add(new CostState(
                Name: cost.Name ?? "",
                TypeId: cost.Type?.Id ?? "",
                Value: (double)(cost.Value * selectedCount)));
        }
        return costs;
    }

    private List<CostState> ComputeRosterCosts(
        List<ForceState> mappedForces,
        IReadOnlySet<string> referencedCostTypeIds)
    {
        // Sum costs from all mapped selections
        var totals = new Dictionary<string, double>(StringComparer.Ordinal);
        foreach (var force in mappedForces)
        {
            AggregateCostsFromSelections(force.Selections, totals);
        }

        // Use roster's cost entries for name/typeId ordering, filter by referenced types
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
    /// Used by the adapter to determine which cost types appear in the roster output.
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
