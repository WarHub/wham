using WarHub.ArmouryModel.Source;

namespace WarHub.ArmouryModel.Concrete;

/// <summary>
/// Validates constraints on roster selections and produces <see cref="Diagnostic"/> objects.
/// Works with ISymbol/SourceNode types from <see cref="WhamCompilation"/>.
/// </summary>
internal sealed class ConstraintValidator
{
    private readonly RosterNode _roster;
    private readonly WhamCompilation _compilation;
    private readonly IReadOnlyList<ICatalogueSymbol> _forceCatalogues;
    private readonly ModifierEvaluator _modifierEvaluator;
    private readonly Dictionary<string, ISelectionEntryContainerSymbol> _entryIndex;
    private readonly Dictionary<string, ICategoryEntrySymbol> _categoryIndex;
    private readonly Dictionary<string, IForceEntrySymbol> _forceEntryIndex;
    // Maps shared entry target ID -> set of link IDs that reference it
    private readonly Dictionary<string, HashSet<string>> _sharedEntryLinkIds;

    private ConstraintValidator(
        RosterNode roster,
        WhamCompilation compilation,
        IReadOnlyList<ICatalogueSymbol> forceCatalogues)
    {
        _roster = roster;
        _compilation = compilation;
        _forceCatalogues = forceCatalogues;
        _modifierEvaluator = new ModifierEvaluator(roster, compilation);
        _entryIndex = new Dictionary<string, ISelectionEntryContainerSymbol>(StringComparer.Ordinal);
        _categoryIndex = new Dictionary<string, ICategoryEntrySymbol>(StringComparer.Ordinal);
        _forceEntryIndex = new Dictionary<string, IForceEntrySymbol>(StringComparer.Ordinal);
        _sharedEntryLinkIds = new Dictionary<string, HashSet<string>>(StringComparer.Ordinal);
        BuildIndex();
    }

    public static void Validate(
        RosterNode roster,
        WhamCompilation compilation,
        DiagnosticBag diagnostics)
    {
        var forceCatalogues = ResolveForceCatalogues(roster, compilation);
        var validator = new ConstraintValidator(roster, compilation, forceCatalogues);
        validator.Run(diagnostics);
    }

    /// <summary>
    /// Resolves the catalogue for each force in the roster by matching
    /// <see cref="ForceNode.CatalogueId"/> against the compilation's
    /// <see cref="IGamesystemNamespaceSymbol.Catalogues"/>.
    /// </summary>
    private static IReadOnlyList<ICatalogueSymbol> ResolveForceCatalogues(
        RosterNode roster,
        WhamCompilation compilation)
    {
        var globalNamespace = compilation.GlobalNamespace;
        var catalogues = globalNamespace.Catalogues;
        var result = new List<ICatalogueSymbol>(roster.Forces.Count);

        foreach (var force in roster.Forces)
        {
            var catalogueId = force.CatalogueId;
            ICatalogueSymbol? matched = null;
            if (catalogueId is not null)
            {
                foreach (var cat in catalogues)
                {
                    if (string.Equals(cat.Id, catalogueId, StringComparison.Ordinal))
                    {
                        matched = cat;
                        break;
                    }
                }
            }
            result.Add(matched ?? globalNamespace.RootCatalogue);
        }

        return result;
    }

    private void Run(DiagnosticBag diagnostics)
    {
        for (int i = 0; i < _roster.Forces.Count; i++)
        {
            var force = _roster.Forces[i];
            ValidateForceSelections(force, i, diagnostics);
        }

        ValidateForceEntryConstraints(diagnostics);
        ValidateCostLimits(diagnostics);
    }

    // ------------------------------------------------------------------
    //  Cost limit validation
    // ------------------------------------------------------------------

    private void ValidateCostLimits(DiagnosticBag diagnostics)
    {
        // Aggregate total costs from roster (already computed in roster node cost limits)
        var totalCosts = new Dictionary<string, decimal>(StringComparer.Ordinal);
        foreach (var force in _roster.Forces)
        {
            AggregateCostsRecursive(force.Selections, totalCosts);
        }

        foreach (var limit in _roster.CostLimits)
        {
            if (limit.Value < 0) continue;
            var actual = totalCosts.GetValueOrDefault(limit.TypeId, 0m);
            if (actual > limit.Value + 0.001m)
            {
                var costName = GetCostTypeName(limit.TypeId);
                diagnostics.Add(ErrorCode.WRN_CostLimitExceeded, Location.None,
                    costName, actual, limit.Value);
            }
        }
    }

    private static void AggregateCostsRecursive(
        IReadOnlyList<SelectionNode> selections,
        Dictionary<string, decimal> totals)
    {
        foreach (var sel in selections)
        {
            foreach (var cost in sel.Costs)
            {
                totals.TryGetValue(cost.TypeId, out var current);
                totals[cost.TypeId] = current + cost.Value;
            }
            AggregateCostsRecursive(sel.Selections, totals);
        }
    }

    private string GetCostTypeName(string typeId)
    {
        var gs = _compilation.GlobalNamespace.RootCatalogue;
        foreach (var rd in gs.ResourceDefinitions)
        {
            if (rd.ResourceKind == ResourceKind.Cost && rd.Id == typeId)
                return rd.Name;
        }
        return typeId;
    }

    // ------------------------------------------------------------------
    //  Force entry constraints (field=forces)
    // ------------------------------------------------------------------

    private void ValidateForceEntryConstraints(DiagnosticBag diagnostics)
    {
        var forceCounts = new Dictionary<string, int>(StringComparer.Ordinal);
        foreach (var force in _roster.Forces)
        {
            forceCounts[force.EntryId] = forceCounts.GetValueOrDefault(force.EntryId) + 1;
        }

        foreach (var force in _roster.Forces)
        {
            if (!_forceEntryIndex.TryGetValue(force.EntryId, out var forceEntry))
                continue;

            foreach (var constraint in forceEntry.Constraints)
            {
                var query = constraint.Query;
                if (query.ValueKind != QueryValueKind.ForceCount) continue;

                var count = query.ScopeKind == QueryScopeKind.ContainingRoster
                    ? forceCounts.GetValueOrDefault(force.EntryId)
                    : 1;
                var constraintValue = query.ReferenceValue ?? 0m;

                bool isMin = query.Comparison == QueryComparisonType.GreaterThanOrEqual;
                bool isMax = query.Comparison == QueryComparisonType.LessThanOrEqual;

                if (isMin && count < constraintValue - 0.001m)
                {
                    diagnostics.Add(ErrorCode.WRN_ForceCountViolation, Location.None,
                        force.EntryId, count, "need at least", constraintValue);
                }
                else if (isMax && constraintValue >= 0 && count > constraintValue + 0.001m)
                {
                    diagnostics.Add(ErrorCode.WRN_ForceCountViolation, Location.None,
                        force.EntryId, count, "allowed at most", constraintValue);
                }
            }
        }
    }

    // ------------------------------------------------------------------
    //  Force selection constraints
    // ------------------------------------------------------------------

    private void ValidateForceSelections(ForceNode force, int forceIndex, DiagnosticBag diagnostics)
    {
        var catalogue = forceIndex < _forceCatalogues.Count
            ? _forceCatalogues[forceIndex]
            : _compilation.GlobalNamespace.RootCatalogue;

        var sharedChecked = new HashSet<(string constraintId, string entryId)>();

        // Walk root-level entries in the catalogue (not descendants)
        // Descendants' constraints are checked in ValidateChildConstraints
        foreach (var entry in GetRootEntries(catalogue))
        {
            var targetId = GetTargetEntryId(entry);
            if (targetId is null) continue;
            var isLink = entry.IsReference && entry.ReferencedEntry is not null;
            var linkId = entry.Id; // link's own ID (same as targetId if not a link)

            // Hidden entry validation: if entry is hidden but has selections, error
            if (IsEffectivelyHidden(entry))
            {
                var cats = entry.Categories;
                if (cats.IsEmpty && entry.ReferencedEntry is { } r)
                    cats = r.Categories;
                if (!cats.IsEmpty)
                {
                    var hiddenCountId = isLink ? linkId! : targetId;
                    var selCount = CountSelectionsInForce(hiddenCountId, force);
                    if (selCount > 0)
                    {
                        diagnostics.Add(ErrorCode.WRN_ConstraintMaxViolation, Location.None,
                            targetId, selCount, 0);
                    }
                }
            }

            // Collect all constraints: link's own + shared target's
            var constraintSources = new List<(IConstraintSymbol constraint, string sourceEntryId, bool isShared)>();
            foreach (var c in entry.Constraints)
                constraintSources.Add((c, linkId ?? targetId, false));
            if (isLink && entry.ReferencedEntry is ISelectionEntryContainerSymbol target)
            {
                foreach (var c in target.Constraints)
                    constraintSources.Add((c, targetId, c.Query.Options.HasFlag(QueryOptions.SharedConstraint)));
            }

            if (constraintSources.Count == 0) continue;

            // Compute effective constraint values (modifiers can change constraint boundaries)
            var effectiveValues = _modifierEvaluator.GetEffectiveConstraintValues(entry, null, force);
            if (isLink && entry.ReferencedEntry is IContainerEntrySymbol refEntry)
            {
                var refValues = _modifierEvaluator.GetEffectiveConstraintValues(refEntry, null, force);
                foreach (var (k, v) in refValues)
                    effectiveValues.TryAdd(k, v);
            }

            // For shared constraints, merge link constraints of the same type
            // and use the most restrictive value.
            // Key by (direction, value kind, cost type) to avoid merging unrelated constraints.
            var mergedShared = new Dictionary<string, (decimal value, string constraintId, QueryComparisonType comparison)>(StringComparer.Ordinal);
            foreach (var (constraint, _, isShared) in constraintSources)
            {
                if (!isShared) continue;
                var query = constraint.Query;
                var cid = constraint.Id ?? "";
                var val = effectiveValues.GetValueOrDefault(cid, query.ReferenceValue ?? 0m);
                var direction = query.Comparison == QueryComparisonType.GreaterThanOrEqual ? "min" : "max";
                var key = $"{direction}:{query.ValueKind}:{query.ValueTypeSymbol?.Id}";
                if (!mergedShared.TryGetValue(key, out var existing))
                {
                    mergedShared[key] = (val, cid, query.Comparison);
                }
                else
                {
                    if (direction == "max" && val < existing.value)
                        mergedShared[key] = (val, existing.constraintId, existing.comparison);
                    else if (direction == "min" && val > existing.value)
                        mergedShared[key] = (val, existing.constraintId, existing.comparison);
                }
            }
            // Also merge link constraints into shared if both exist with same type
            if (mergedShared.Count > 0)
            {
                foreach (var (constraint, _, isShared) in constraintSources)
                {
                    if (isShared) continue;
                    var query = constraint.Query;
                    var cid = constraint.Id ?? "";
                    var val = effectiveValues.GetValueOrDefault(cid, query.ReferenceValue ?? 0m);
                    var direction = query.Comparison == QueryComparisonType.GreaterThanOrEqual ? "min" : "max";
                    var key = $"{direction}:{query.ValueKind}:{query.ValueTypeSymbol?.Id}";
                    if (mergedShared.TryGetValue(key, out var existing))
                    {
                        if (direction == "max" && val < existing.value)
                            mergedShared[key] = (val, existing.constraintId, existing.comparison);
                        else if (direction == "min" && val > existing.value)
                            mergedShared[key] = (val, existing.constraintId, existing.comparison);
                    }
                }
            }
            // Track which link constraint IDs were merged into shared
            var mergedLinkConstraintIds = new HashSet<string>(StringComparer.Ordinal);
            if (mergedShared.Count > 0)
            {
                foreach (var (constraint, _, isShared) in constraintSources)
                {
                    if (isShared) continue;
                    var query = constraint.Query;
                    var direction = query.Comparison == QueryComparisonType.GreaterThanOrEqual ? "min" : "max";
                    var key = $"{direction}:{query.ValueKind}:{query.ValueTypeSymbol?.Id}";
                    if (mergedShared.ContainsKey(key) && constraint.Id is not null)
                        mergedLinkConstraintIds.Add(constraint.Id);
                }
            }

            foreach (var (constraint, sourceEntryId, isShared) in constraintSources)
            {
                var query = constraint.Query;
                var constraintId = constraint.Id ?? "";

                // Skip link constraints that were merged into shared
                if (!isShared && mergedLinkConstraintIds.Contains(constraintId))
                    continue;

                // Shared constraint: validate once per (constraintId, entryId)
                if (isShared || query.Options.HasFlag(QueryOptions.SharedConstraint))
                {
                    if (!sharedChecked.Add((constraintId, targetId))) continue;
                }

                // field=forces on selection entry: BS always counts 0 forces
                if (query.ValueKind == QueryValueKind.ForceCount)
                {
                    var forceConstraintValue = effectiveValues.GetValueOrDefault(constraintId, query.ReferenceValue ?? 0m);
                    AddConstraintError(query.Comparison, forceConstraintValue, 0, sourceEntryId, diagnostics);
                    continue;
                }

                // Count selections or costs in scope
                // For shared constraints, count across ALL links to same shared entry
                var countId = isLink ? linkId! : targetId;
                var useSharedCounting = isShared || query.Options.HasFlag(QueryOptions.SharedConstraint);
                decimal count;
                if (query.ValueKind == QueryValueKind.SelectionCount)
                {
                    count = useSharedCounting
                        ? CountSharedSelectionsInScope(query, targetId, force)
                        : CountSelectionsInScope(query, countId, force);
                }
                else if (query.ValueKind == QueryValueKind.MemberValue)
                {
                    count = CountCostInScope(query, countId, force);
                }
                else
                {
                    continue;
                }

                // Use merged value for shared constraints, raw value otherwise
                decimal constraintValue;
                if (isShared)
                {
                    var direction = query.Comparison == QueryComparisonType.GreaterThanOrEqual ? "min" : "max";
                    var mergeKey = $"{direction}:{query.ValueKind}:{query.ValueTypeSymbol?.Id}";
                    constraintValue = mergedShared.TryGetValue(mergeKey, out var m)
                        ? m.value
                        : effectiveValues.GetValueOrDefault(constraintId, query.ReferenceValue ?? 0m);
                }
                else
                {
                    constraintValue = effectiveValues.GetValueOrDefault(constraintId, query.ReferenceValue ?? 0m);
                }

                // Percent value support
                if (query.Options.HasFlag(QueryOptions.ValuePercentage))
                {
                    var total = CountTotalSelectionsInScope(query, force);
                    constraintValue = total * constraintValue / 100m;
                    if (query.Options.HasFlag(QueryOptions.ValueRoundUp))
                        constraintValue = Math.Ceiling(constraintValue);
                }

                // For shared constraints, attribute error to shared entry
                var errorEntryId = isShared ? targetId : sourceEntryId;

                AddConstraintError(query.Comparison, constraintValue, count, errorEntryId, diagnostics);
            }
        }

        // Validate child selection constraints
        foreach (var sel in force.Selections)
        {
            ValidateChildConstraints(sel, force, diagnostics);
        }

        // Validate category constraints
        ValidateCategoryConstraints(force, diagnostics);
    }

    private void ValidateChildConstraints(
        SelectionNode parent,
        ForceNode force,
        DiagnosticBag diagnostics)
    {
        // Count children by entry ID
        var childCounts = new Dictionary<string, int>(StringComparer.Ordinal);
        foreach (var child in parent.Selections)
        {
            var id = child.EntryId;
            childCounts[id] = childCounts.GetValueOrDefault(id) + child.Number;
        }

        // Check constraints on child entries (scope=parent)
        foreach (var child in parent.Selections)
        {
            if (!_entryIndex.TryGetValue(child.EntryId, out var childEntry))
                continue;

            var effectiveChildValues = _modifierEvaluator.GetEffectiveConstraintValues(childEntry, child, force);

            foreach (var constraint in childEntry.Constraints)
            {
                var query = constraint.Query;
                if (query.ValueKind != QueryValueKind.SelectionCount) continue;
                if (query.ScopeKind != QueryScopeKind.Parent) continue;

                var constraintId = constraint.Id ?? "";
                var constraintValue = effectiveChildValues.GetValueOrDefault(constraintId, query.ReferenceValue ?? 0m);
                var count = childCounts.GetValueOrDefault(child.EntryId);

                AddConstraintError(query.Comparison, constraintValue, count, child.EntryId, diagnostics);
            }
        }

        // Also check entries that are available but have 0 selections (min violations)
        if (_entryIndex.TryGetValue(parent.EntryId, out var parentEntry))
        {
            foreach (var childEntrySymbol in parentEntry.ChildSelectionEntries)
            {
                var childId = GetTargetEntryId(childEntrySymbol);
                if (childId is null) continue;
                if (childCounts.ContainsKey(childId)) continue; // already counted above

                var targetEntry = childEntrySymbol.ReferencedEntry ?? childEntrySymbol;
                var effectiveAvailValues = _modifierEvaluator.GetEffectiveConstraintValues(targetEntry, null, force);

                foreach (var constraint in targetEntry.Constraints)
                {
                    var query = constraint.Query;
                    if (query.ValueKind != QueryValueKind.SelectionCount) continue;
                    if (query.ScopeKind != QueryScopeKind.Parent) continue;

                    var constraintId = constraint.Id ?? "";
                    var constraintValue = effectiveAvailValues.GetValueOrDefault(constraintId, query.ReferenceValue ?? 0m);
                    AddConstraintError(query.Comparison, constraintValue, 0, childId, diagnostics);
                }
            }
        }

        // Recurse
        foreach (var child in parent.Selections)
        {
            ValidateChildConstraints(child, force, diagnostics);
        }
    }

    private void ValidateCategoryConstraints(ForceNode force, DiagnosticBag diagnostics)
    {
        // Count selections per category
        var categoryCounts = new Dictionary<string, int>(StringComparer.Ordinal);
        foreach (var sel in FlattenSelections(force.Selections))
        {
            foreach (var cat in sel.Categories)
            {
                categoryCounts[cat.EntryId] =
                    categoryCounts.GetValueOrDefault(cat.EntryId) + sel.Number;
            }
        }

        // Check force entry category link constraints
        if (!_forceEntryIndex.TryGetValue(force.EntryId, out var forceEntry))
            return;

        foreach (var catLink in forceEntry.Categories)
        {
            var targetEntry = catLink.ReferencedEntry ?? catLink;
            var catTargetId = targetEntry.Id;
            if (catTargetId is null) continue;

            foreach (var constraint in catLink.Constraints)
            {
                var query = constraint.Query;
                if (query.ValueKind != QueryValueKind.SelectionCount) continue;

                var constraintValue = query.ReferenceValue ?? 0m;
                var count = categoryCounts.GetValueOrDefault(catTargetId);

                if (query.Comparison == QueryComparisonType.GreaterThanOrEqual
                    && count < constraintValue - 0.001m && constraintValue > 0)
                {
                    diagnostics.Add(ErrorCode.WRN_CategoryCountViolation, Location.None,
                        catTargetId, count, "need at least", constraintValue);
                }
                else if (query.Comparison == QueryComparisonType.LessThanOrEqual
                    && count > constraintValue + 0.001m)
                {
                    diagnostics.Add(ErrorCode.WRN_CategoryCountViolation, Location.None,
                        catTargetId, count, "allowed at most", constraintValue);
                }
            }
        }
    }

    // ------------------------------------------------------------------
    //  Counting helpers
    // ------------------------------------------------------------------

    private decimal CountSelectionsInScope(IQuerySymbol query, string targetId, ForceNode force)
    {
        bool includeChildren = query.Options.HasFlag(QueryOptions.IncludeDescendantSelections);
        var selections = query.ScopeKind switch
        {
            QueryScopeKind.Parent or QueryScopeKind.ContainingForce =>
                includeChildren ? FlattenSelections(force.Selections) : TopLevelSelections(force),
            QueryScopeKind.ContainingRoster =>
                includeChildren ? AllSelectionsFlattened() : AllTopLevelSelections(),
            _ => includeChildren ? FlattenSelections(force.Selections) : TopLevelSelections(force),
        };

        decimal count = 0;
        foreach (var sel in selections)
        {
            if (sel.EntryId == targetId)
                count += sel.Number;
        }
        return count;
    }

    /// <summary>
    /// Counts selections for shared constraints: sums across ALL entry links
    /// pointing to the same shared entry.
    /// </summary>
    private decimal CountSharedSelectionsInScope(IQuerySymbol query, string sharedEntryId, ForceNode force)
    {
        // Collect all IDs that map to this shared entry
        var matchIds = new HashSet<string>(StringComparer.Ordinal) { sharedEntryId };
        if (_sharedEntryLinkIds.TryGetValue(sharedEntryId, out var linkIds))
        {
            foreach (var lid in linkIds)
                matchIds.Add(lid);
        }

        bool includeChildren = query.Options.HasFlag(QueryOptions.IncludeDescendantSelections);
        var selections = query.ScopeKind switch
        {
            QueryScopeKind.Parent or QueryScopeKind.ContainingForce =>
                includeChildren ? FlattenSelections(force.Selections) : TopLevelSelections(force),
            QueryScopeKind.ContainingRoster =>
                includeChildren ? AllSelectionsFlattened() : AllTopLevelSelections(),
            _ => includeChildren ? FlattenSelections(force.Selections) : TopLevelSelections(force),
        };

        decimal count = 0;
        foreach (var sel in selections)
        {
            if (matchIds.Contains(sel.EntryId))
                count += sel.Number;
        }
        return count;
    }

    private decimal CountCostInScope(IQuerySymbol query, string targetId, ForceNode force)
    {
        bool includeChildren = query.Options.HasFlag(QueryOptions.IncludeDescendantSelections);
        var costTypeId = query.ValueTypeSymbol?.Id;
        if (costTypeId is null) return 0m;

        var selections = query.ScopeKind switch
        {
            QueryScopeKind.Parent or QueryScopeKind.ContainingForce =>
                includeChildren ? FlattenSelections(force.Selections) : TopLevelSelections(force),
            QueryScopeKind.ContainingRoster =>
                includeChildren ? AllSelectionsFlattened() : AllTopLevelSelections(),
            _ => includeChildren ? FlattenSelections(force.Selections) : TopLevelSelections(force),
        };

        decimal sum = 0;
        foreach (var sel in selections)
        {
            if (sel.EntryId != targetId) continue;
            foreach (var cost in sel.Costs)
            {
                if (cost.TypeId == costTypeId)
                    sum += cost.Value;
            }
        }
        return sum;
    }

    private decimal CountTotalSelectionsInScope(IQuerySymbol query, ForceNode force)
    {
        bool includeChildren = query.Options.HasFlag(QueryOptions.IncludeDescendantSelections);
        var selections = query.ScopeKind switch
        {
            QueryScopeKind.Parent or QueryScopeKind.ContainingForce =>
                includeChildren ? FlattenSelections(force.Selections) : TopLevelSelections(force),
            QueryScopeKind.ContainingRoster =>
                includeChildren ? AllSelectionsFlattened() : AllTopLevelSelections(),
            _ => includeChildren ? FlattenSelections(force.Selections) : TopLevelSelections(force),
        };

        decimal count = 0;
        foreach (var sel in selections)
            count += sel.Number;
        return count;
    }

    private IEnumerable<SelectionNode> TopLevelSelections(ForceNode force) => force.Selections;

    private IEnumerable<SelectionNode> AllTopLevelSelections() =>
        _roster.Forces.SelectMany(f => f.Selections);

    private IEnumerable<SelectionNode> AllSelectionsFlattened() =>
        _roster.Forces.SelectMany(f => FlattenSelections(f.Selections));

    private static int CountSelectionsInForce(string entryId, ForceNode force)
    {
        int count = 0;
        foreach (var sel in FlattenSelections(force.Selections))
        {
            if (sel.EntryId == entryId)
                count += sel.Number;
        }
        return count;
    }

    private static bool IsEffectivelyHidden(ISelectionEntryContainerSymbol entry)
    {
        if (entry.IsHidden) return true;
        if (entry.ReferencedEntry is { IsHidden: true }) return true;
        return false;
    }

    private static IEnumerable<SelectionNode> FlattenSelections(IReadOnlyList<SelectionNode> selections)
    {
        foreach (var sel in selections)
        {
            yield return sel;
            foreach (var desc in FlattenSelections(sel.Selections))
                yield return desc;
        }
    }

    // ------------------------------------------------------------------
    //  Constraint checking
    // ------------------------------------------------------------------

    private static void AddConstraintError(
        QueryComparisonType comparison,
        decimal constraintValue,
        decimal count,
        string entryId,
        DiagnosticBag diagnostics)
    {
        bool isMin = comparison == QueryComparisonType.GreaterThanOrEqual;
        bool isMax = comparison == QueryComparisonType.LessThanOrEqual;

        if (isMin && count < constraintValue - 0.001m)
        {
            diagnostics.Add(ErrorCode.WRN_ConstraintMinViolation, Location.None,
                entryId, count, constraintValue);
        }
        else if (isMax && constraintValue >= 0 && count > constraintValue + 0.001m)
        {
            diagnostics.Add(ErrorCode.WRN_ConstraintMaxViolation, Location.None,
                entryId, count, constraintValue);
        }
    }

    // ------------------------------------------------------------------
    //  Index building
    // ------------------------------------------------------------------

    private void BuildIndex()
    {
        var gs = _compilation.GlobalNamespace.RootCatalogue;
        IndexCatalogue(gs);
        foreach (var cat in _forceCatalogues)
        {
            IndexCatalogue(cat);
        }
    }

    private void IndexCatalogue(ICatalogueSymbol catalogue)
    {
        foreach (var entry in catalogue.RootContainerEntries)
        {
            if (entry is ISelectionEntryContainerSymbol selEntry)
                IndexEntry(selEntry);
            if (entry is IForceEntrySymbol fe)
                _forceEntryIndex.TryAdd(fe.Id, fe);
        }
        foreach (var entry in catalogue.SharedSelectionEntryContainers)
            IndexEntry(entry);
    }

    private void IndexEntry(ISelectionEntryContainerSymbol entry)
    {
        var targetId = GetTargetEntryId(entry);
        if (targetId is not null)
            _entryIndex.TryAdd(targetId, entry);

        // Also index by link ID if different
        if (entry.Id != targetId && entry.Id is not null)
        {
            _entryIndex.TryAdd(entry.Id, entry);
            // Track link -> shared entry mapping for shared constraint counting
            if (targetId is not null)
            {
                if (!_sharedEntryLinkIds.TryGetValue(targetId, out var linkIds))
                {
                    linkIds = new HashSet<string>(StringComparer.Ordinal);
                    _sharedEntryLinkIds[targetId] = linkIds;
                }
                linkIds.Add(entry.Id);
            }
        }

        // Index category entries
        foreach (var cat in entry.Categories)
        {
            var catTarget = cat.ReferencedEntry ?? cat;
            if (catTarget.Id is not null)
                _categoryIndex.TryAdd(catTarget.Id, catTarget);
        }

        // Recurse into children
        foreach (var child in entry.ChildSelectionEntries)
            IndexEntry(child);
    }

    private static string? GetTargetEntryId(ISelectionEntryContainerSymbol entry)
    {
        // For entry links, the target entry ID is the referenced entry's ID
        if (entry.ReferencedEntry is { } referenced)
            return referenced.Id;
        return entry.Id;
    }

    private static IEnumerable<ISelectionEntryContainerSymbol> GetRootEntries(ICatalogueSymbol catalogue)
    {
        foreach (var entry in catalogue.RootContainerEntries)
        {
            if (entry is ISelectionEntryContainerSymbol selEntry)
                yield return selEntry;
        }
    }

    private static IEnumerable<ISelectionEntryContainerSymbol> GetAllEntries(ICatalogueSymbol catalogue)
    {
        foreach (var entry in catalogue.RootContainerEntries)
        {
            if (entry is not ISelectionEntryContainerSymbol selEntry) continue;
            yield return selEntry;
            foreach (var child in GetDescendantEntries(selEntry))
                yield return child;
        }
        foreach (var entry in catalogue.SharedSelectionEntryContainers)
        {
            yield return entry;
            foreach (var child in GetDescendantEntries(entry))
                yield return child;
        }
    }

    private static IEnumerable<ISelectionEntryContainerSymbol> GetDescendantEntries(
        ISelectionEntryContainerSymbol entry)
    {
        foreach (var child in entry.ChildSelectionEntries)
        {
            yield return child;
            foreach (var desc in GetDescendantEntries(child))
                yield return desc;
        }
    }
}

internal static class ValidationProperties
{
    public const string OwnerType = "ownerType";
    public const string OwnerId = "ownerId";
    public const string OwnerEntryId = "ownerEntryId";
    public const string EntryId = "entryId";
    public const string ConstraintId = "constraintId";
}
