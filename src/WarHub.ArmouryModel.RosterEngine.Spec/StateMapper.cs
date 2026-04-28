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
    /// <summary>
    /// Numeric-aware string comparer matching NewRecruit's locale-aware ordering.
    /// "Unit 2" sorts before "Unit 10" (numeric segment comparison).
    /// </summary>
    private static readonly Comparison<string?> NumericAwareComparison = static (a, b) =>
    {
        if (ReferenceEquals(a, b)) return 0;
        if (a is null) return -1;
        if (b is null) return 1;

        int ia = 0, ib = 0;
        while (ia < a.Length && ib < b.Length)
        {
            if (char.IsDigit(a[ia]) && char.IsDigit(b[ib]))
            {
                // Compare numeric segments by value
                int startA = ia, startB = ib;
                while (ia < a.Length && char.IsDigit(a[ia])) ia++;
                while (ib < b.Length && char.IsDigit(b[ib])) ib++;
                var numA = long.Parse(a.AsSpan(startA, ia - startA));
                var numB = long.Parse(b.AsSpan(startB, ib - startB));
                var cmp = numA.CompareTo(numB);
                if (cmp != 0) return cmp;
            }
            else
            {
                var cmp = a[ia].CompareTo(b[ib]);
                if (cmp != 0) return cmp;
                ia++;
                ib++;
            }
        }
        return a.Length.CompareTo(b.Length);
    };

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
        // Sort forces alphabetically by name (BattleScribe canonical ordering)
        forces.Sort(static (a, b) => NumericAwareComparison(a.Name, b.Name));

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

    private ForceState MapForce(IForceSymbol force, int availableEntryCount)
    {
        // Build category order map for NR-style selection sorting
        var categoryOrder = new Dictionary<string, int>(StringComparer.Ordinal);
        for (int i = 0; i < force.Categories.Length; i++)
        {
            var catEntryId = force.Categories[i].SourceEntry?.Id;
            if (catEntryId is not null)
                categoryOrder[catEntryId] = i;
        }

        var sortedSelections = force.Selections
            .Order(Comparer<ISelectionSymbol>.Create((a, b) =>
            {
                var aCatId = a.PrimaryCategory?.SourceEntry?.Id;
                var bCatId = b.PrimaryCategory?.SourceEntry?.Id;
                int aOrder = aCatId is not null && categoryOrder.TryGetValue(aCatId, out var ao) ? ao : -1;
                int bOrder = bCatId is not null && categoryOrder.TryGetValue(bCatId, out var bo) ? bo : -1;
                var cmp = aOrder.CompareTo(bOrder);
                if (cmp != 0) return cmp;
                return NumericAwareComparison(a.SourceEntry?.Name ?? a.Name, b.SourceEntry?.Name ?? b.Name);
            }));
        var selections = new List<SelectionState>();
        foreach (var sel in sortedSelections)
        {
            selections.Add(MapSelection(sel));
        }

        var profiles = MapProfiles(force.EffectiveSourceEntry.Resources);
        var rules = MapRules(force.EffectiveSourceEntry.Resources);
        var categories = MapForceCategories(force);
        var publications = MapPublications(force);
        var childForces = MapChildForces(force);

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

    private SelectionState MapSelection(ISelectionSymbol sel)
    {
        // Sort child selections by original (pre-modifier) entry name with numeric awareness (NR canonical ordering)
        var sortedChildren = sel.Selections
            .Order(Comparer<ISelectionSymbol>.Create((a, b) =>
            {
                try
                {
                    return NumericAwareComparison(a.SourceEntry?.Name ?? a.Name, b.SourceEntry?.Name ?? b.Name);
                }
                catch
                {
                    return NumericAwareComparison(a.Name, b.Name);
                }
            }));
        var children = new List<SelectionState>();
        foreach (var child in sortedChildren)
        {
            children.Add(MapSelection(child));
        }

        var eff = sel.EffectiveSourceEntry;

        var costs = MapSelectionCosts(eff, sel.SelectedCount);
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

        // Get the declaration to access EntryGroupId, CustomName, CustomNotes
        var decl = sel.GetDeclaration();

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
            EntryGroupId: decl?.EntryGroupId,
            CustomName: decl?.CustomName,
            CustomNotes: decl?.CustomNotes);
    }

    // ──────────────────────────────────────────────────────────────────
    //  Profile / Rule mapping from effective symbols
    // ──────────────────────────────────────────────────────────────────

    private static List<ProfileState> MapProfiles(ImmutableArray<IResourceEntrySymbol> resources)
    {
        var result = new List<ProfileState>();
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
        var result = new List<RuleState>();
        foreach (var resource in resources)
        {
            if (resource is not IRuleSymbol r)
                continue;
            if (r.IsHidden)
                continue;
            result.Add(new RuleState(
                Name: r.Name ?? "",
                Description: r.DescriptionText,
                Hidden: r.IsHidden,
                Page: r.Page,
                PublicationId: r.PublicationReference?.PublicationId));
        }
        return result;
    }

    // ──────────────────────────────────────────────────────────────────
    //  Category mapping
    // ──────────────────────────────────────────────────────────────────

    private static List<CategoryState> MapSelectionCategories(ISelectionEntryContainerSymbol eff)
    {
        var categories = new List<CategoryState>();
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
        var categories = new List<CategoryState>();
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
        var result = new List<PublicationState>();
        var seen = new HashSet<string>(StringComparer.Ordinal);

        // Catalogue-specific publications first
        var catalogue = force.CatalogueReference.Catalogue;
        if (!catalogue.IsGamesystem)
        {
            foreach (var def in catalogue.ResourceDefinitions)
            {
                if (def is IPublicationSymbol pub && pub.Id is not null && seen.Add(pub.Id))
                    result.Add(new PublicationState(Id: pub.Id, Name: pub.Name ?? ""));
            }
        }

        // Then game system publications
        var gamesystem = catalogue.Gamesystem;
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

    private List<ForceState> MapChildForces(IForceSymbol force)
    {
        var childForces = new List<ForceState>();
        foreach (var child in force.Forces)
        {
            childForces.Add(MapForce(child, 0));
        }
        return childForces;
    }

    // ──────────────────────────────────────────────────────────────────
    //  Cost mapping
    // ──────────────────────────────────────────────────────────────────

    private static List<CostState> MapSelectionCosts(ISelectionEntryContainerSymbol eff, int selectedCount)
    {
        var costs = new List<CostState>();
        foreach (var cost in eff.Costs)
        {
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
