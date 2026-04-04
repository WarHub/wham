using BattleScribeSpec;
using WarHub.ArmouryModel.Concrete;
using WarHub.ArmouryModel.Source;

namespace WarHub.ArmouryModel.RosterEngine.Spec;

/// <summary>
/// Validates constraints on roster selections and generates validation errors.
/// Works with ISymbol/SourceNode types from Compilation.
/// </summary>
internal sealed class ConstraintValidator
{
    private readonly RosterNode _roster;
    private readonly Compilation _compilation;
    private readonly IReadOnlyList<ICatalogueSymbol> _forceCatalogues;
    private readonly EffectiveEntryCache _effectiveCache;
    private readonly Dictionary<string, ISelectionEntryContainerSymbol> _entryIndex;
    private readonly Dictionary<string, ICategoryEntrySymbol> _categoryIndex;
    private readonly Dictionary<string, IForceEntrySymbol> _forceEntryIndex;
    // Maps shared entry target ID → set of link IDs that reference it
    private readonly Dictionary<string, HashSet<string>> _sharedEntryLinkIds;

    // Node → Symbol lookups (built lazily from the compilation's symbol tree)
    private Dictionary<ForceNode, IForceSymbol>? _forceSymbols;
    private Dictionary<SelectionNode, ISelectionSymbol>? _selectionSymbols;

    private ConstraintValidator(
        RosterNode roster,
        Compilation compilation,
        IReadOnlyList<ICatalogueSymbol> forceCatalogues)
    {
        _roster = roster;
        _compilation = compilation;
        _forceCatalogues = forceCatalogues;
        // Get or create the effective entry cache from the roster symbol.
        // The cache is self-initializing — it creates its own ModifierEvaluator.
        var whamCompilation = (WhamCompilation)compilation;
        var rosterSymbol = whamCompilation.SourceGlobalNamespace.Rosters
            .FirstOrDefault(r => r.Declaration == roster)
            ?? whamCompilation.SourceGlobalNamespace.Rosters.FirstOrDefault();
        _effectiveCache = rosterSymbol!.GetOrCreateEffectiveEntryCache();
        _entryIndex = new Dictionary<string, ISelectionEntryContainerSymbol>(StringComparer.Ordinal);
        _categoryIndex = new Dictionary<string, ICategoryEntrySymbol>(StringComparer.Ordinal);
        _forceEntryIndex = new Dictionary<string, IForceEntrySymbol>(StringComparer.Ordinal);
        _sharedEntryLinkIds = new Dictionary<string, HashSet<string>>(StringComparer.Ordinal);
        BuildIndex();
    }

    private void EnsureSymbolLookup()
    {
        if (_forceSymbols is not null) return;
        _forceSymbols = new Dictionary<ForceNode, IForceSymbol>();
        _selectionSymbols = new Dictionary<SelectionNode, ISelectionSymbol>();
        var whamCompilation = (WhamCompilation)_compilation;
        foreach (var rosterSym in whamCompilation.SourceGlobalNamespace.Rosters)
        {
            foreach (var forceSym in rosterSym.Forces)
                IndexForce(forceSym);
        }
    }

    private void IndexForce(ForceSymbol forceSym)
    {
        _forceSymbols![forceSym.Declaration] = forceSym;
        foreach (var selSym in forceSym.ChildSelections)
            IndexSelection(selSym);
        foreach (var childForce in forceSym.Forces)
            IndexForce(childForce);
    }

    private void IndexSelection(SelectionSymbol selSym)
    {
        _selectionSymbols![selSym.Declaration] = selSym;
        foreach (var childSel in selSym.ChildSelections)
            IndexSelection(childSel);
    }

    private ISelectionSymbol? LookupSelection(SelectionNode? node)
    {
        if (node is null) return null;
        EnsureSymbolLookup();
        return _selectionSymbols!.GetValueOrDefault(node);
    }

    private IForceSymbol? LookupForce(ForceNode? node)
    {
        if (node is null) return null;
        EnsureSymbolLookup();
        return _forceSymbols!.GetValueOrDefault(node);
    }

    public static List<ValidationErrorState> Validate(
        RosterNode roster,
        Compilation compilation,
        IReadOnlyList<ICatalogueSymbol> forceCatalogues)
    {
        var validator = new ConstraintValidator(roster, compilation, forceCatalogues);
        return validator.Run();
    }

    private List<ValidationErrorState> Run()
    {
        var errors = new List<ValidationErrorState>();

        for (int i = 0; i < _roster.Forces.Count; i++)
        {
            var force = _roster.Forces[i];
            ValidateForceSelections(force, i, errors);
        }

        ValidateForceEntryConstraints(errors);
        ValidateCostLimits(errors);

        return errors;
    }

    // ──────────────────────────────────────────────────────────────────
    //  Cost limit validation
    // ──────────────────────────────────────────────────────────────────

    private void ValidateCostLimits(List<ValidationErrorState> errors)
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
                errors.Add(new ValidationErrorState(
                    Message: $"Cost {costName} ({actual}) exceeds limit ({limit.Value})",
                    OwnerType: "roster",
                    EntryId: "costLimits",
                    ConstraintId: limit.TypeId));
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

    // ──────────────────────────────────────────────────────────────────
    //  Force entry constraints (field=forces)
    // ──────────────────────────────────────────────────────────────────

    private void ValidateForceEntryConstraints(List<ValidationErrorState> errors)
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

                CheckConstraint(query.Comparison, constraintValue, count,
                    force.EntryId, "roster", null, constraint.Id ?? "", errors);
            }
        }
    }

    // ──────────────────────────────────────────────────────────────────
    //  Force selection constraints
    // ──────────────────────────────────────────────────────────────────

    private void ValidateForceSelections(ForceNode force, int forceIndex, List<ValidationErrorState> errors)
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
                        errors.Add(new ValidationErrorState(
                            Message: $"Entry {targetId} is hidden but has {selCount} selection(s)",
                            OwnerType: "selection",
                            OwnerEntryId: targetId,
                            EntryId: targetId,
                            ConstraintId: "hidden"));
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
            var effectiveValues = GetEffectiveConstraintValues(entry, null, force);
            if (isLink && entry.ReferencedEntry is IContainerEntrySymbol refEntry)
            {
                var refValues = GetEffectiveConstraintValues(refEntry, null, force);
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
                    CheckConstraint(query.Comparison, forceConstraintValue, 0,
                        sourceEntryId, "roster", null, constraintId, errors);
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

                var (ownerType, ownerEntryId) = GetOwnerForConstraint(
                    query.Comparison, query.ScopeKind, entry, targetId);

                // For shared constraints, attribute error to shared entry
                var errorEntryId = isShared ? targetId : sourceEntryId;
                var errorConstraintId = constraintId;
                if (isShared)
                {
                    var direction = query.Comparison == QueryComparisonType.GreaterThanOrEqual ? "min" : "max";
                    var mergeKey = $"{direction}:{query.ValueKind}:{query.ValueTypeSymbol?.Id}";
                    if (mergedShared.TryGetValue(mergeKey, out var m))
                        errorConstraintId = m.constraintId;
                }

                CheckConstraint(query.Comparison, constraintValue, count,
                    errorEntryId, ownerType, ownerEntryId, errorConstraintId, errors);
            }
        }

        // Validate child selection constraints
        foreach (var sel in force.Selections)
        {
            ValidateChildConstraints(sel, force, errors);
        }

        // Validate category constraints
        ValidateCategoryConstraints(force, errors);
    }

    private void ValidateChildConstraints(
        SelectionNode parent,
        ForceNode force,
        List<ValidationErrorState> errors)
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

            var effectiveChildValues = GetEffectiveConstraintValues(childEntry, child, force);

            foreach (var constraint in childEntry.Constraints)
            {
                var query = constraint.Query;
                if (query.ValueKind != QueryValueKind.SelectionCount) continue;
                if (query.ScopeKind != QueryScopeKind.Parent) continue;

                var constraintId = constraint.Id ?? "";
                var constraintValue = effectiveChildValues.GetValueOrDefault(constraintId, query.ReferenceValue ?? 0m);
                var count = childCounts.GetValueOrDefault(child.EntryId);

                var parentEntryId = parent.EntryId;
                CheckConstraint(query.Comparison, constraintValue, count,
                    child.EntryId, "selection", parentEntryId,
                    constraintId, errors);
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
                var effectiveAvailValues = GetEffectiveConstraintValues(targetEntry, null, force);

                foreach (var constraint in targetEntry.Constraints)
                {
                    var query = constraint.Query;
                    if (query.ValueKind != QueryValueKind.SelectionCount) continue;
                    if (query.ScopeKind != QueryScopeKind.Parent) continue;

                    var constraintId = constraint.Id ?? "";
                    var constraintValue = effectiveAvailValues.GetValueOrDefault(constraintId, query.ReferenceValue ?? 0m);
                    CheckConstraint(query.Comparison, constraintValue, 0,
                        childId, "selection", parent.EntryId,
                        constraintId, errors);
                }
            }
        }

        // Recurse
        foreach (var child in parent.Selections)
        {
            ValidateChildConstraints(child, force, errors);
        }
    }

    private void ValidateCategoryConstraints(ForceNode force, List<ValidationErrorState> errors)
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
                    errors.Add(new ValidationErrorState(
                        Message: $"Min {constraintValue} required for category {catTargetId}, have {count}",
                        OwnerType: "category",
                        OwnerEntryId: catTargetId,
                        EntryId: catTargetId,
                        ConstraintId: constraint.Id));
                }
                else if (query.Comparison == QueryComparisonType.LessThanOrEqual
                    && count > constraintValue + 0.001m)
                {
                    errors.Add(new ValidationErrorState(
                        Message: $"Max {constraintValue} allowed for category {catTargetId}, have {count}",
                        OwnerType: "category",
                        OwnerEntryId: catTargetId,
                        EntryId: catTargetId,
                        ConstraintId: constraint.Id));
                }
            }
        }
    }

    // ──────────────────────────────────────────────────────────────────
    //  Counting helpers
    // ──────────────────────────────────────────────────────────────────

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

    // ──────────────────────────────────────────────────────────────────
    //  Owner determination
    // ──────────────────────────────────────────────────────────────────

    private static (string ownerType, string? ownerEntryId) GetOwnerForConstraint(
        QueryComparisonType comparison,
        QueryScopeKind scopeKind,
        ISelectionEntryContainerSymbol entry,
        string entryId)
    {
        bool isMin = comparison == QueryComparisonType.GreaterThanOrEqual;

        // scope=force + min → force owner
        if (scopeKind == QueryScopeKind.ContainingForce && isMin)
            return ("force", null);

        // min constraints with category links → category owner
        if (isMin)
        {
            var cats = entry.Categories;
            if (cats.IsEmpty && entry.ReferencedEntry is { } referenced)
                cats = referenced.Categories;

            if (!cats.IsEmpty)
            {
                var primary = cats.FirstOrDefault(c => c.IsPrimaryCategory);
                var catEntry = primary?.ReferencedEntry ?? primary;
                if (catEntry is not null)
                    return ("category", catEntry.Id);
                var firstCat = cats[0].ReferencedEntry ?? cats[0];
                return ("category", firstCat.Id);
            }
        }

        // All other → selection owner
        return ("selection", entryId);
    }

    // ──────────────────────────────────────────────────────────────────
    //  Constraint checking
    // ──────────────────────────────────────────────────────────────────

    private static void CheckConstraint(
        QueryComparisonType comparison,
        decimal constraintValue,
        decimal count,
        string entryId,
        string ownerType,
        string? ownerEntryId,
        string constraintId,
        List<ValidationErrorState> errors)
    {
        bool isMin = comparison == QueryComparisonType.GreaterThanOrEqual;
        bool isMax = comparison == QueryComparisonType.LessThanOrEqual;

        if (isMin && count < constraintValue - 0.001m)
        {
            errors.Add(new ValidationErrorState(
                Message: $"Min {constraintValue} required for {entryId}, have {count}",
                OwnerType: ownerType,
                OwnerEntryId: ownerEntryId,
                EntryId: entryId,
                ConstraintId: constraintId));
        }
        else if (isMax && constraintValue >= 0 && count > constraintValue + 0.001m)
        {
            errors.Add(new ValidationErrorState(
                Message: $"Max {constraintValue} allowed for {entryId}, have {count}",
                OwnerType: ownerType,
                OwnerEntryId: ownerEntryId,
                EntryId: entryId,
                ConstraintId: constraintId));
        }
    }

    // ──────────────────────────────────────────────────────────────────
    //  Index building
    // ──────────────────────────────────────────────────────────────────

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
            // Track link → shared entry mapping for shared constraint counting
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

    /// <summary>
    /// Gets effective constraint values from the <see cref="_effectiveCache"/>.
    /// Returns a dictionary of constraint ID → effective ReferenceValue,
    /// extracted from the effective entry's constraints.
    /// </summary>
    private Dictionary<string, decimal> GetEffectiveConstraintValues(
        IContainerEntrySymbol entry,
        SelectionNode? selection,
        ForceNode? force)
    {
        if (entry is ISelectionEntryContainerSymbol sec)
        {
            var effectiveEntry = _effectiveCache.GetEffectiveEntry(sec, LookupSelection(selection), LookupForce(force));
            var result = new Dictionary<string, decimal>(StringComparer.Ordinal);
            foreach (var constraint in effectiveEntry.Constraints)
            {
                if (constraint.Id is { } id)
                {
                    result[id] = constraint.Query.ReferenceValue ?? 0m;
                }
            }
            return result;
        }
        // Fallback: return declared values for non-selection-entry containers
        var fallback = new Dictionary<string, decimal>(StringComparer.Ordinal);
        foreach (var constraint in entry.Constraints)
        {
            if (constraint.Id is { } id)
            {
                fallback[id] = constraint.Query.ReferenceValue ?? 0m;
            }
        }
        return fallback;
    }
}
