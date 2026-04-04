using BattleScribeSpec;
using BattleScribeSpec.Protocol;
using WarHub.ArmouryModel.Concrete;
using WarHub.ArmouryModel.EditorServices;
using WarHub.ArmouryModel.Source;
using ProtocolRosterState = BattleScribeSpec.RosterState;

namespace WarHub.ArmouryModel.RosterEngine.Spec;

/// <summary>
/// Maps the ISymbol-based roster tree (SourceNode/WhamCompilation) to BattleScribeSpec Protocol types.
/// This is the reverse of <see cref="ProtocolConverter"/> — it reads the immutable roster tree
/// and produces the Protocol state expected by the spec test harness.
/// </summary>
internal sealed class StateMapper
{
    private readonly WhamCompilation _compilation;
    private readonly EntryResolver _resolver;
    private readonly IReadOnlyList<ICatalogueSymbol> _forceCatalogues;
    private readonly EffectiveEntryCache _effectiveCache;
    private readonly NodeSymbolLookup _nodeSymbols;

    // ISymbol entry lookup (entryId → IEntrySymbol)
    private Dictionary<string, IEntrySymbol>? _symbolEntries;

    public StateMapper(Compilation compilation, RosterNode roster, IReadOnlyList<ICatalogueSymbol> forceCatalogues)
    {
        _compilation = (WhamCompilation)compilation;
        _resolver = new EntryResolver();
        _forceCatalogues = forceCatalogues;
        _nodeSymbols = new NodeSymbolLookup(_compilation);
        // Get or create the effective entry cache from the roster symbol.
        // The cache is self-initializing — it creates its own ModifierEvaluator.
        var rosterSymbol = _compilation.SourceGlobalNamespace.Rosters
            .FirstOrDefault(r => r.Declaration == roster)
            ?? _compilation.SourceGlobalNamespace.Rosters.FirstOrDefault();
        _effectiveCache = rosterSymbol!.GetOrCreateEffectiveEntryCache();
    }

    public ProtocolRosterState MapRosterState(RosterNode roster)
    {
        var gsSym = _compilation.GlobalNamespace.RootCatalogue;

        var forces = new List<ForceState>();
        for (int i = 0; i < roster.Forces.Count; i++)
        {
            var catalogue = i < _forceCatalogues.Count ? _forceCatalogues[i] : gsSym;
            forces.Add(MapForce(roster.Forces[i], catalogue));
        }

        // Compute roster-level cost totals from effective selection costs (modifier-aware)
        var costs = ComputeRosterCostsFromSelections(roster, forces);

        // Phase 5: Constraint validation (from compilation diagnostics)
        var errors = GetConstraintErrors();

        return new ProtocolRosterState(
            Name: roster.Name ?? "New Roster",
            GameSystemId: roster.GameSystemId ?? gsSym.Id,
            Forces: forces,
            Costs: costs,
            ValidationErrors: errors);
    }

    private ForceState MapForce(ForceNode forceNode, ICatalogueSymbol catalogue)
    {
        var selections = new List<SelectionState>();
        foreach (var selNode in forceNode.Selections)
        {
            selections.Add(MapSelection(selNode, forceNode));
        }

        var availableEntries = _resolver.GetAvailableEntries(catalogue);

        // Resolve profiles and rules from the force entry via Symbol layer
        var forceSym = _nodeSymbols.GetForce(forceNode);
        List<ProfileState> profiles;
        List<RuleState> rules;
        if (forceSym?.SourceEntry is { } forceEntry)
        {
            var (resolvedProfiles, resolvedRules) = _effectiveCache.GetEffectiveResources(forceEntry, null, forceSym);
            profiles = MapResolvedProfiles(resolvedProfiles);
            rules = MapResolvedRules(resolvedRules);
        }
        else
        {
            profiles = MapNodeProfiles(forceNode.Profiles);
            rules = MapNodeRules(forceNode.Rules);
        }

        return new ForceState(
            Name: forceNode.Name ?? "",
            CatalogueId: forceNode.CatalogueId,
            Selections: selections,
            AvailableEntryCount: availableEntries.Count,
            PublicationId: forceNode.PublicationId,
            Page: forceNode.Page)
        {
            Profiles = profiles.Count > 0 ? profiles : [],
            Rules = rules.Count > 0 ? rules : [],
        };
    }

    private SelectionState MapSelection(SelectionNode selNode, ForceNode force)
    {
        var children = new List<SelectionState>();
        foreach (var childNode in selNode.Selections)
        {
            children.Add(MapSelection(childNode, force));
        }

        // Look up symbols once for this selection
        var selSym = _nodeSymbols.GetSelection(selNode);
        var forceSym = _nodeSymbols.GetForce(force);

        // Look up the ISymbol for this entry to access effects (modifiers)
        var entrySym = LookupEntrySymbol(selNode.EntryId);

        // Use effective entry from cache for modifier-applied values
        var effectiveEntry = entrySym is ISelectionEntryContainerSymbol sec
            ? _effectiveCache.GetEffectiveEntry(sec, selSym, forceSym)
            : null;

        var effectiveName = effectiveEntry is not null
            ? effectiveEntry.Name
            : selNode.Name ?? "";
        var effectiveHidden = effectiveEntry is not null
            ? effectiveEntry.IsHidden
            : entrySym is not null
                ? _effectiveCache.Evaluator.GetEffectiveHidden(entrySym, selSym, forceSym)
                : false;

        // Costs: use Symbol-layer effective costs × SelectedCount
        List<CostState> effectiveCosts;
        if (effectiveEntry is not null && selSym is not null)
        {
            effectiveCosts = MapResolvedCosts(_effectiveCache.GetEffectiveSelectionCosts(effectiveEntry, selSym));
        }
        else
        {
            effectiveCosts = MapSelectionCosts(selNode);
        }

        // Categories: use Symbol-layer effective categories with modifier application
        List<CategoryState> categories;
        if (entrySym is ISelectionEntryContainerSymbol secCat && selSym is not null)
        {
            try
            {
                categories = MapResolvedCategories(
                    _effectiveCache.GetEffectiveSelectionCategories(secCat, selSym, forceSym));
            }
            catch (InvalidCastException)
            {
                categories = MapSelectionCategories(selNode);
            }
        }
        else
        {
            categories = MapSelectionCategories(selNode);
        }

        // Profiles and rules: use Symbol-layer resolution
        List<ProfileState> profiles;
        List<RuleState> rules;
        if (entrySym is not null)
        {
            var (resolvedProfiles, resolvedRules) = _effectiveCache.GetEffectiveResources(entrySym, selSym, forceSym);
            profiles = MapResolvedProfiles(resolvedProfiles);
            rules = MapResolvedRules(resolvedRules);
        }
        else
        {
            profiles = MapNodeProfiles(selNode.Profiles);
            rules = MapNodeRules(selNode.Rules);
        }

        var type = selNode.Type switch
        {
            SelectionEntryKind.Unit => "unit",
            SelectionEntryKind.Model => "model",
            SelectionEntryKind.Upgrade => "upgrade",
            _ => "upgrade",
        };

        // Publication: use Symbol-layer resolution
        var publicationName = _effectiveCache.ResolvePublicationName(selNode.PublicationId);

        // Page: apply modifiers if entry symbol is available
        var effectivePage = selNode.Page;
        if (entrySym is not null)
        {
            var modPage = _effectiveCache.Evaluator.GetEffectivePage(entrySym, selSym, forceSym);
            if (modPage is not null)
                effectivePage = modPage;
        }

        return new SelectionState(
            Name: effectiveName,
            EntryId: selNode.EntryId,
            Type: type,
            Number: selNode.Number,
            Hidden: effectiveHidden,
            Costs: effectiveCosts,
            Children: children,
            Profiles: profiles.Count > 0 ? profiles : null,
            Rules: rules.Count > 0 ? rules : null,
            Categories: categories.Count > 0 ? categories : null,
            Page: effectivePage,
            PublicationId: selNode.PublicationId,
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

    private static List<ProfileState> MapNodeProfiles(ListNode<ProfileNode> profiles)
    {
        var result = new List<ProfileState>();
        foreach (var p in profiles)
        {
            var chars = new List<CharacteristicState>();
            foreach (var ch in p.Characteristics)
            {
                chars.Add(new CharacteristicState(
                    Name: ch.Name ?? "",
                    TypeId: ch.TypeId ?? "",
                    Value: ch.Value ?? ""));
            }
            result.Add(new ProfileState(
                Name: p.Name ?? "",
                TypeId: p.TypeId,
                TypeName: p.TypeName,
                Hidden: p.Hidden,
                Characteristics: chars,
                Page: p.Page,
                PublicationId: p.PublicationId));
        }
        return result;
    }

    private static List<RuleState> MapNodeRules(ListNode<RuleNode> rules)
    {
        var result = new List<RuleState>();
        foreach (var r in rules)
        {
            result.Add(new RuleState(
                Name: r.Name ?? "",
                Description: r.Description ?? "",
                Hidden: r.Hidden,
                Page: r.Page,
                PublicationId: r.PublicationId));
        }
        return result;
    }

    // ──────────────────────────────────────────────────────────────────
    //  Cost mapping (fallback for selections without symbol entries)
    // ──────────────────────────────────────────────────────────────────

    private static List<CostState> MapSelectionCosts(SelectionNode selNode)
    {
        var costs = new List<CostState>();
        foreach (var costNode in selNode.Costs)
        {
            var value = (double)(costNode.Value * selNode.Number);
            costs.Add(new CostState(
                Name: costNode.Name ?? "",
                TypeId: costNode.TypeId ?? "",
                Value: value));
        }
        return costs;
    }

    private static List<CategoryState> MapSelectionCategories(SelectionNode selNode)
    {
        var categories = new List<CategoryState>();
        foreach (var catNode in selNode.Categories)
        {
            categories.Add(new CategoryState(
                Name: catNode.Name ?? "",
                EntryId: catNode.EntryId,
                Primary: catNode.Primary));
        }
        return categories;
    }

    // ──────────────────────────────────────────────────────────────────
    //  ISymbol entry lookup
    // ──────────────────────────────────────────────────────────────────

    private IEntrySymbol? LookupEntrySymbol(string? entryId)
    {
        if (string.IsNullOrEmpty(entryId)) return null;
        EnsureSymbolEntryLookup();
        return _symbolEntries!.GetValueOrDefault(entryId);
    }

    private void EnsureSymbolEntryLookup()
    {
        if (_symbolEntries is not null) return;
        _symbolEntries = new(StringComparer.Ordinal);

        foreach (var catalogue in _compilation.GlobalNamespace.Catalogues)
        {
            IndexSymbolEntries(catalogue);
        }
        IndexSymbolEntries(_compilation.GlobalNamespace.RootCatalogue);
    }

    private void IndexSymbolEntries(ICatalogueSymbol catalogue)
    {
        foreach (var entry in catalogue.RootContainerEntries)
        {
            if (entry is ISelectionEntryContainerSymbol sec)
                IndexSymbolEntryRecursive(sec);
            else if (entry.Id is not null)
                _symbolEntries![entry.Id] = entry;
        }
        foreach (var entry in catalogue.SharedSelectionEntryContainers)
        {
            IndexSymbolEntryRecursive(entry);
        }
        // Index shared profiles and rules as IEntrySymbol (for modifier lookup)
        foreach (var res in catalogue.SharedResourceEntries)
        {
            if (res.Id is not null)
                _symbolEntries!.TryAdd(res.Id, res);
            // If profile, also index characteristics
            if (res is IProfileSymbol profile)
            {
                foreach (var ch in profile.Characteristics)
                {
                    if (ch.Id is not null)
                        _symbolEntries!.TryAdd(ch.Id, ch);
                }
            }
        }
        // Index root resource entries (profiles at catalogue root)
        foreach (var res in catalogue.RootResourceEntries)
        {
            if (res.Id is not null)
                _symbolEntries!.TryAdd(res.Id, res);
            if (res is IProfileSymbol profile)
            {
                foreach (var ch in profile.Characteristics)
                {
                    if (ch.Id is not null)
                        _symbolEntries!.TryAdd(ch.Id, ch);
                }
            }
        }
        // Note: We don't iterate AllItems as it may trigger lazy binding that crashes.
        // The above indexing should cover all needed entries.
    }

    private void IndexSymbolEntryRecursive(ISelectionEntryContainerSymbol entry)
    {
        if (entry.Id is not null)
            _symbolEntries![entry.Id] = entry;
        // Index sub-entries (profiles, rules, infolinks, infogroups) from this entry's resources
        IndexResourceEntriesRecursive(entry.Resources);
        foreach (var child in entry.ChildSelectionEntries)
        {
            IndexSymbolEntryRecursive(child);
        }
    }

    private void IndexResourceEntriesRecursive(ImmutableArray<IResourceEntrySymbol> resources)
    {
        foreach (var res in resources)
        {
            if (res.Id is not null)
                _symbolEntries!.TryAdd(res.Id, res);
            // If profile, also index characteristics
            if (res is IProfileSymbol profile)
            {
                foreach (var ch in profile.Characteristics)
                {
                    if (ch.Id is not null)
                        _symbolEntries!.TryAdd(ch.Id, ch);
                }
            }
            // Recursively index resources within InfoGroups
            if (res.Resources.Length > 0)
            {
                IndexResourceEntriesRecursive(res.Resources);
            }
        }
    }

    private List<CostState> MapRosterCosts(RosterNode roster)
    {
        // Collect cost types referenced by any available entry in any force's catalogue
        var referencedTypes = new HashSet<string>(StringComparer.Ordinal);
        foreach (var catalogue in _forceCatalogues)
        {
            var entries = _resolver.GetAvailableEntries(catalogue);
            foreach (var entry in entries)
            {
                CollectReferencedCostTypes(entry.Symbol, referencedTypes);
            }
        }

        var result = new List<CostState>();
        foreach (var cost in roster.Costs)
        {
            if (referencedTypes.Contains(cost.TypeId ?? ""))
            {
                result.Add(new CostState(
                    Name: cost.Name ?? "",
                    TypeId: cost.TypeId ?? "",
                    Value: (double)cost.Value));
            }
        }
        return result;
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
