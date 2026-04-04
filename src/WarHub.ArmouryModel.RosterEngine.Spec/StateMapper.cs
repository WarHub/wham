using BattleScribeSpec;
using BattleScribeSpec.Protocol;
using WarHub.ArmouryModel.Concrete;
using WarHub.ArmouryModel.EditorServices;
using WarHub.ArmouryModel.Source;
using ProtocolRosterState = BattleScribeSpec.RosterState;

namespace WarHub.ArmouryModel.RosterEngine.Spec;

/// <summary>
/// Maps the ISymbol-based roster tree to BattleScribeSpec Protocol types.
/// Walks the symbol tree directly — all business logic (modifier evaluation,
/// profile/rule resolution, category/cost computation) lives in the Symbol layer.
/// </summary>
internal sealed class StateMapper
{
    private readonly WhamCompilation _compilation;
    private readonly EntryResolver _resolver;
    private readonly IReadOnlyList<ICatalogueSymbol> _forceCatalogues;
    private readonly EffectiveEntryCache _effectiveCache;
    private readonly RosterSymbol _rosterSymbol;

    public StateMapper(Compilation compilation, RosterNode roster, IReadOnlyList<ICatalogueSymbol> forceCatalogues)
    {
        _compilation = (WhamCompilation)compilation;
        _resolver = new EntryResolver();
        _forceCatalogues = forceCatalogues;
        _rosterSymbol = _compilation.SourceGlobalNamespace.Rosters
            .FirstOrDefault(r => r.Declaration == roster)
            ?? _compilation.SourceGlobalNamespace.Rosters.First();
        _effectiveCache = _rosterSymbol.GetOrCreateEffectiveEntryCache();
    }

    public ProtocolRosterState MapRosterState(RosterNode roster)
    {
        var gsSym = _compilation.GlobalNamespace.RootCatalogue;

        var forces = new List<ForceState>();
        for (int i = 0; i < _rosterSymbol.Forces.Length; i++)
        {
            var catalogue = i < _forceCatalogues.Count ? _forceCatalogues[i] : gsSym;
            forces.Add(MapForce(_rosterSymbol.Forces[i], catalogue));
        }

        var costs = ComputeRosterCostsFromSelections(roster, forces);
        var errors = GetConstraintErrors();

        return new ProtocolRosterState(
            Name: roster.Name ?? "New Roster",
            GameSystemId: roster.GameSystemId ?? gsSym.Id,
            Forces: forces,
            Costs: costs,
            ValidationErrors: errors);
    }

    private ForceState MapForce(ForceSymbol forceSym, ICatalogueSymbol catalogue)
    {
        var selections = new List<SelectionState>();
        foreach (var selSym in forceSym.ChildSelections)
        {
            selections.Add(MapSelection(selSym, forceSym));
        }

        var availableEntries = _resolver.GetAvailableEntries(catalogue);

        var (resolvedProfiles, resolvedRules) = _effectiveCache.GetEffectiveResources(
            forceSym.SourceEntry, null, forceSym);

        return new ForceState(
            Name: forceSym.Declaration.Name ?? "",
            CatalogueId: forceSym.Declaration.CatalogueId,
            Selections: selections,
            AvailableEntryCount: availableEntries.Count,
            PublicationId: forceSym.Declaration.PublicationId,
            Page: forceSym.Declaration.Page)
        {
            Profiles = resolvedProfiles.Count > 0 ? MapResolvedProfiles(resolvedProfiles) : [],
            Rules = resolvedRules.Count > 0 ? MapResolvedRules(resolvedRules) : [],
        };
    }

    private SelectionState MapSelection(SelectionSymbol selSym, IForceSymbol forceSym)
    {
        var children = new List<SelectionState>();
        foreach (var childSym in selSym.ChildSelections)
        {
            children.Add(MapSelection(childSym, forceSym));
        }

        // Get entry symbol and effective entry from Symbol layer
        var entrySym = selSym.SourceEntry as ISelectionEntryContainerSymbol;
        var effectiveEntry = entrySym is not null
            ? _effectiveCache.GetEffectiveEntry(entrySym, selSym, forceSym)
            : null;

        var effectiveName = effectiveEntry?.Name ?? selSym.Declaration.Name ?? "";
        var effectiveHidden = effectiveEntry?.IsHidden ?? false;

        // Costs: effective per-unit costs × SelectedCount
        var costs = effectiveEntry is not null
            ? MapResolvedCosts(_effectiveCache.GetEffectiveSelectionCosts(effectiveEntry, selSym))
            : MapDeclaredCosts(selSym);

        // Categories: apply entry modifiers to selection's runtime categories
        List<CategoryState> categories;
        if (entrySym is not null)
        {
            try
            {
                categories = MapResolvedCategories(
                    _effectiveCache.GetEffectiveSelectionCategories(entrySym, selSym, forceSym));
            }
            catch (InvalidCastException)
            {
                categories = MapDeclaredCategories(selSym);
            }
        }
        else
        {
            categories = MapDeclaredCategories(selSym);
        }

        // Profiles and rules from Symbol-layer ResourceResolver
        var sourceEntry = (IEntrySymbol?)entrySym ?? selSym.SourceEntry;
        var (resolvedProfiles, resolvedRules) = _effectiveCache.GetEffectiveResources(sourceEntry, selSym, forceSym);
        var profiles = MapResolvedProfiles(resolvedProfiles);
        var rules = MapResolvedRules(resolvedRules);

        var type = selSym.EntryKind switch
        {
            SelectionEntryKind.Unit => "unit",
            SelectionEntryKind.Model => "model",
            SelectionEntryKind.Upgrade => "upgrade",
            _ => "upgrade",
        };

        // Publication and page from Symbol layer
        var publicationName = _effectiveCache.ResolvePublicationName(selSym.Declaration.PublicationId);
        var effectivePage = selSym.Declaration.Page;
        if (sourceEntry is not null)
        {
            var modPage = _effectiveCache.Evaluator.GetEffectivePage(sourceEntry, selSym, forceSym);
            if (modPage is not null)
                effectivePage = modPage;
        }

        return new SelectionState(
            Name: effectiveName,
            EntryId: selSym.Declaration.EntryId,
            Type: type,
            Number: selSym.SelectedCount,
            Hidden: effectiveHidden,
            Costs: costs,
            Children: children,
            Profiles: profiles.Count > 0 ? profiles : null,
            Rules: rules.Count > 0 ? rules : null,
            Categories: categories.Count > 0 ? categories : null,
            Page: effectivePage,
            PublicationId: selSym.Declaration.PublicationId,
            PublicationName: publicationName);
    }

    private static List<ProfileState> MapResolvedProfiles(IReadOnlyList<ResolvedProfile> resolved)
    {
        var result = new List<ProfileState>(resolved.Count);
        foreach (var p in resolved)
        {
            var chars = new List<CharacteristicState>(p.Characteristics.Length);
            foreach (var ch in p.Characteristics)
            {
                chars.Add(new CharacteristicState(
                    Name: ch.Name,
                    TypeId: ch.TypeId ?? "",
                    Value: ch.Value));
            }
            result.Add(new ProfileState(
                Name: p.Name,
                TypeId: p.TypeId,
                TypeName: p.TypeName,
                Hidden: p.Hidden,
                Characteristics: chars,
                Page: p.Page,
                PublicationId: p.PublicationId));
        }
        return result;
    }

    private static List<RuleState> MapResolvedRules(IReadOnlyList<ResolvedRule> resolved)
    {
        var result = new List<RuleState>(resolved.Count);
        foreach (var r in resolved)
        {
            result.Add(new RuleState(
                Name: r.Name,
                Description: r.Description,
                Hidden: r.Hidden,
                Page: r.Page,
                PublicationId: r.PublicationId));
        }
        return result;
    }

    private static List<CategoryState> MapResolvedCategories(IReadOnlyList<ResolvedCategory> resolved)
    {
        var result = new List<CategoryState>(resolved.Count);
        foreach (var c in resolved)
        {
            result.Add(new CategoryState(
                Name: c.Name,
                EntryId: c.EntryId,
                Primary: c.IsPrimary));
        }
        return result;
    }

    private static List<CostState> MapResolvedCosts(IReadOnlyList<ResolvedCost> resolved)
    {
        var result = new List<CostState>(resolved.Count);
        foreach (var c in resolved)
        {
            result.Add(new CostState(
                Name: c.Name,
                TypeId: c.TypeId,
                Value: c.Value));
        }
        return result;
    }

    private static List<CostState> MapDeclaredCosts(ISelectionSymbol selSym)
    {
        var costs = new List<CostState>();
        foreach (var cost in selSym.Costs)
        {
            costs.Add(new CostState(
                Name: cost.Name ?? "",
                TypeId: cost.Type?.Id ?? "",
                Value: (double)(cost.Value * selSym.SelectedCount)));
        }
        return costs;
    }

    private static List<CategoryState> MapDeclaredCategories(ISelectionSymbol selSym)
    {
        var categories = new List<CategoryState>();
        foreach (var cat in selSym.Categories)
        {
            categories.Add(new CategoryState(
                Name: cat.SourceEntry?.Name ?? "",
                EntryId: cat.SourceEntry?.Id ?? "",
                Primary: cat.IsPrimaryCategory));
        }
        return categories;
    }

    private List<CostState> ComputeRosterCostsFromSelections(RosterNode roster, List<ForceState> mappedForces)
    {
        // Collect cost types referenced by any available entry
        var referencedTypes = new HashSet<string>(StringComparer.Ordinal);
        var costNames = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var catalogue in _forceCatalogues)
        {
            var entries = _resolver.GetAvailableEntries(catalogue);
            foreach (var entry in entries)
            {
                CollectReferencedCostTypes(entry.Symbol, referencedTypes);
            }
        }
        // Collect cost type names from the roster node
        foreach (var cost in roster.Costs)
        {
            if (cost.TypeId is not null && cost.Name is not null)
                costNames[cost.TypeId] = cost.Name;
        }

        // Sum costs from all mapped selections (which have effective/modified values)
        var totals = new Dictionary<string, double>(StringComparer.Ordinal);
        foreach (var force in mappedForces)
        {
            AggregateCostsFromSelections(force.Selections, totals);
        }

        // Build result, only including referenced cost types
        var result = new List<CostState>();
        foreach (var cost in roster.Costs)
        {
            var typeId = cost.TypeId ?? "";
            if (referencedTypes.Contains(typeId))
            {
                result.Add(new CostState(
                    Name: cost.Name ?? "",
                    TypeId: typeId,
                    Value: totals.GetValueOrDefault(typeId, 0)));
            }
        }
        return result;
    }

    private static void AggregateCostsFromSelections(IReadOnlyList<SelectionState> selections, Dictionary<string, double> totals)
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

    private static void CollectReferencedCostTypes(ISelectionEntryContainerSymbol symbol, HashSet<string> types)
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


    private IReadOnlyList<ValidationErrorState> GetConstraintErrors()
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
}
