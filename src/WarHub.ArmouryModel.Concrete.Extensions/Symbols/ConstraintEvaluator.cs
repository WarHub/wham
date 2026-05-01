// Ported code: many BattleScribe IDs are string? but dictionary keys are string.
// Suppress nullable warnings at file level until IDs are properly typed.
#pragma warning disable CS8604 // Possible null reference argument
#pragma warning disable CS8620 // Nullability differences in argument

using WarHub.ArmouryModel.Source;

namespace WarHub.ArmouryModel.Concrete;

/// <summary>
/// Evaluates constraints on roster selections and generates diagnostics.
/// Works directly with Symbol types from Compilation.
/// </summary>
internal static class ConstraintEvaluator
{
    public static void Evaluate(RosterSymbol roster, WhamCompilation compilation, DiagnosticBag diagnostics)
    {
        var evaluator = new Evaluator(roster, compilation, diagnostics);
        evaluator.Run();
    }

    private sealed class Evaluator
    {
        private enum CountKeyKind { Entry, Group }
        private readonly record struct CountKey(string Id, CountKeyKind Kind);

        private readonly RosterSymbol _roster;
        private readonly WhamCompilation _compilation;
        private readonly DiagnosticBag _diagnostics;
        private readonly EffectiveEntryCache _effectiveCache;
        private readonly Dictionary<string, ISelectionEntryContainerSymbol> _entryIndex;
        private readonly Dictionary<string, ICategoryEntrySymbol> _categoryIndex;
        private readonly Dictionary<string, IForceEntrySymbol> _forceEntryIndex;
        // Maps shared entry target ID → set of link IDs that reference it
        private readonly Dictionary<string, HashSet<string>> _sharedEntryLinkIds;

        public Evaluator(
            RosterSymbol roster,
            WhamCompilation compilation,
            DiagnosticBag diagnostics)
        {
            _roster = roster;
            _compilation = compilation;
            _diagnostics = diagnostics;
            _effectiveCache = roster.GetOrCreateEffectiveEntryCache();
            _entryIndex = new Dictionary<string, ISelectionEntryContainerSymbol>(StringComparer.Ordinal);
            _categoryIndex = new Dictionary<string, ICategoryEntrySymbol>(StringComparer.Ordinal);
            _forceEntryIndex = new Dictionary<string, IForceEntrySymbol>(StringComparer.Ordinal);
            _sharedEntryLinkIds = new Dictionary<string, HashSet<string>>(StringComparer.Ordinal);
            BuildIndex();
        }

        public void Run()
        {
            for (int i = 0; i < _roster.Forces.Length; i++)
            {
                var force = _roster.Forces[i];
                ValidateForceSelections(force, i);
            }

            ValidateForceEntryConstraints();
            ValidateCostLimits();
        }

        // ──────────────────────────────────────────────────────────────────
        //  Cost limit validation
        // ──────────────────────────────────────────────────────────────────

        private void ValidateCostLimits()
        {
            // Aggregate total costs from roster
            var totalCosts = new Dictionary<string, decimal>(StringComparer.Ordinal);
            foreach (var force in _roster.Forces)
            {
                AggregateCostsRecursive(force.ChildSelections, totalCosts);
            }

            foreach (var limit in _roster.Declaration.CostLimits)
            {
                if (limit.Value < 0) continue;
                var actual = totalCosts.GetValueOrDefault(limit.TypeId, 0m);
                if (actual > limit.Value + 0.001m)
                {
                    var costName = GetCostTypeName(limit.TypeId);
                    _diagnostics.Add(ErrorCode.WRN_ExceedsCostLimit, Location.None,
                        "roster", "", "costLimits", limit.TypeId);
                }
            }
        }

        private static void AggregateCostsRecursive(
            ImmutableArray<SelectionSymbol> selections,
            Dictionary<string, decimal> totals)
        {
            foreach (var sel in selections)
            {
                foreach (var cost in sel.Declaration.Costs)
                {
                    totals.TryGetValue(cost.TypeId, out var current);
                    totals[cost.TypeId] = current + cost.Value;
                }
                AggregateCostsRecursive(sel.ChildSelections, totals);
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

        private void ValidateForceEntryConstraints()
        {
            var forceCounts = new Dictionary<string, int>(StringComparer.Ordinal);
            foreach (var force in _roster.Forces)
            {
                var entryId = force.Declaration.EntryId;
                forceCounts[entryId] = forceCounts.GetValueOrDefault(entryId) + 1;
            }

            foreach (var force in _roster.Forces)
            {
                var entryId = force.Declaration.EntryId;
                if (!_forceEntryIndex.TryGetValue(entryId, out var forceEntry))
                    continue;

                foreach (var constraint in forceEntry.Constraints)
                {
                    var query = constraint.Query;
                    if (query.ValueKind != QueryValueKind.ForceCount) continue;

                    var count = query.ScopeKind == QueryScopeKind.ContainingRoster
                        ? forceCounts.GetValueOrDefault(entryId)
                        : 1;
                    var constraintValue = query.ReferenceValue ?? 0m;

                    CheckConstraint(query.Comparison, constraintValue, count,
                        entryId, "roster", null, constraint.Id ?? "");
                }
            }
        }

        // ──────────────────────────────────────────────────────────────────
        //  Force selection constraints
        // ──────────────────────────────────────────────────────────────────

        private void ValidateForceSelections(ForceSymbol force, int forceIndex)
        {
            var forceCatalogues = _roster.Forces
                .Select(f => f.CatalogueReference.Catalogue)
                .ToList();

            var catalogue = forceIndex < forceCatalogues.Count
                ? forceCatalogues[forceIndex]
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
                if (IsEffectivelyHidden(entry, force))
                {
                    var hiddenCountId = isLink ? linkId! : targetId;
                    var selCount = CountSelectionsInForce(hiddenCountId, force);
                    if (selCount > 0)
                    {
                        _diagnostics.Add(ErrorCode.WRN_MaxSelectionCountViolation, Location.None,
                            "selection", targetId, targetId, "hidden");
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
                            sourceEntryId, "roster", null, constraintId);
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
                        errorEntryId, ownerType, ownerEntryId, errorConstraintId);
                }
            }

            // Validate child selection constraints
            foreach (var sel in force.ChildSelections)
            {
                ValidateChildConstraints(sel, force);
            }

            // Validate category constraints
            ValidateCategoryConstraints(force);
        }

        private void ValidateChildConstraints(
            SelectionSymbol parent,
            ForceSymbol force)
        {
            // Count children by entry ID and entry group ID
            var counts = new Dictionary<CountKey, int>();
            foreach (var child in parent.ChildSelections)
            {
                var entryKey = new CountKey(child.Declaration.EntryId, CountKeyKind.Entry);
                counts[entryKey] = counts.GetValueOrDefault(entryKey) + child.SelectedCount;
                var groupId = child.Declaration.EntryGroupId;
                if (groupId is not null)
                {
                    var groupKey = new CountKey(groupId, CountKeyKind.Group);
                    counts[groupKey] = counts.GetValueOrDefault(groupKey) + child.SelectedCount;
                }
            }

            // Check constraints on child entries (scope=parent)
            foreach (var child in parent.ChildSelections)
            {
                if (!TryGetIndexedEntry(child.Declaration.EntryId, out var childEntry))
                    continue;

                var effectiveChildValues = GetEffectiveConstraintValues(childEntry, child, force);

                foreach (var constraint in childEntry.Constraints)
                {
                    var query = constraint.Query;
                    if (query.ValueKind != QueryValueKind.SelectionCount) continue;
                    if (query.ScopeKind != QueryScopeKind.Parent) continue;

                    var constraintId = constraint.Id ?? "";
                    var constraintValue = effectiveChildValues.GetValueOrDefault(constraintId, query.ReferenceValue ?? 0m);
                    var count = counts.GetValueOrDefault(new CountKey(child.Declaration.EntryId, CountKeyKind.Entry));

                    var parentEntryId = parent.Declaration.EntryId;
                    CheckConstraint(query.Comparison, constraintValue, count,
                        child.Declaration.EntryId, "selection", parentEntryId,
                        constraintId);
                }
            }

            // Also check entries that are available but have 0 selections (min violations)
            if (TryGetIndexedEntry(parent.Declaration.EntryId, out var parentEntry))
            {
                foreach (var childEntrySymbol in parentEntry.ChildSelectionEntries)
                {
                    var childId = GetTargetEntryId(childEntrySymbol);
                    if (childId is null) continue;

                    // For groups, count by entryGroupId; for entries, count by entryId
                    var isGroup = childEntrySymbol is ISelectionEntryGroupSymbol;
                    if (counts.ContainsKey(new CountKey(childId, isGroup ? CountKeyKind.Group : CountKeyKind.Entry))) continue;

                    var targetEntry = childEntrySymbol.ReferencedEntry ?? childEntrySymbol;
                    var effectiveAvailValues = GetEffectiveConstraintValues(targetEntry, null, force);

                    foreach (var constraint in targetEntry.Constraints)
                    {
                        var query = constraint.Query;
                        if (query.ValueKind != QueryValueKind.SelectionCount) continue;
                        if (query.ScopeKind != QueryScopeKind.Parent) continue;

                        var constraintId = constraint.Id ?? "";
                        var constraintValue = effectiveAvailValues.GetValueOrDefault(constraintId, query.ReferenceValue ?? 0m);
                        var count = counts.GetValueOrDefault(new CountKey(childId, isGroup ? CountKeyKind.Group : CountKeyKind.Entry));
                        CheckConstraint(query.Comparison, constraintValue, count,
                            childId, "selection", parent.Declaration.EntryId,
                            constraintId);
                    }
                }
            }

            // Recurse
            foreach (var child in parent.ChildSelections)
            {
                ValidateChildConstraints(child, force);
            }
        }

        private void ValidateCategoryConstraints(ForceSymbol force)
        {
            // Count selections per category
            var categoryCounts = new Dictionary<string, int>(StringComparer.Ordinal);
            foreach (var sel in FlattenSelections(force.ChildSelections))
            {
                foreach (var cat in sel.Declaration.Categories)
                {
                    categoryCounts[cat.EntryId] =
                        categoryCounts.GetValueOrDefault(cat.EntryId) + sel.SelectedCount;
                }
            }

            // Check force entry category link constraints
            var forceEntryId = force.Declaration.EntryId;
            if (!_forceEntryIndex.TryGetValue(forceEntryId, out var forceEntry))
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
                        _diagnostics.Add(ErrorCode.WRN_MinCategoryCountViolation, Location.None,
                            "category", catTargetId, catTargetId, constraint.Id ?? "");
                    }
                    else if (query.Comparison == QueryComparisonType.LessThanOrEqual
                        && count > constraintValue + 0.001m)
                    {
                        _diagnostics.Add(ErrorCode.WRN_MaxCategoryCountViolation, Location.None,
                            "category", catTargetId, catTargetId, constraint.Id ?? "");
                    }
                }
            }
        }

        // ──────────────────────────────────────────────────────────────────
        //  Counting helpers
        // ──────────────────────────────────────────────────────────────────

        private decimal CountSelectionsInScope(IQuerySymbol query, string targetId, ForceSymbol force)
        {
            bool includeChildren = query.Options.HasFlag(QueryOptions.IncludeDescendantSelections);
            var selections = query.ScopeKind switch
            {
                QueryScopeKind.Parent or QueryScopeKind.ContainingForce =>
                    includeChildren ? FlattenSelections(force.ChildSelections) : TopLevelSelections(force),
                QueryScopeKind.ContainingRoster =>
                    includeChildren ? AllSelectionsFlattened() : AllTopLevelSelections(),
                _ => includeChildren ? FlattenSelections(force.ChildSelections) : TopLevelSelections(force),
            };

            decimal count = 0;
            foreach (var sel in selections)
            {
                if (EntryIdMatches(sel.Declaration.EntryId, targetId))
                    count += sel.SelectedCount;
            }
            return count;
        }

        /// <summary>
        /// Counts selections for shared constraints: sums across ALL entry links
        /// pointing to the same shared entry.
        /// </summary>
        private decimal CountSharedSelectionsInScope(IQuerySymbol query, string sharedEntryId, ForceSymbol force)
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
                    includeChildren ? FlattenSelections(force.ChildSelections) : TopLevelSelections(force),
                QueryScopeKind.ContainingRoster =>
                    includeChildren ? AllSelectionsFlattened() : AllTopLevelSelections(),
                _ => includeChildren ? FlattenSelections(force.ChildSelections) : TopLevelSelections(force),
            };

            decimal count = 0;
            foreach (var sel in selections)
            {
                if (EntryIdMatchesAny(sel.Declaration.EntryId, matchIds))
                    count += sel.SelectedCount;
            }
            return count;
        }

        private decimal CountCostInScope(IQuerySymbol query, string targetId, ForceSymbol force)
        {
            bool includeChildren = query.Options.HasFlag(QueryOptions.IncludeDescendantSelections);
            var costTypeId = query.ValueTypeSymbol?.Id;
            if (costTypeId is null) return 0m;

            var selections = query.ScopeKind switch
            {
                QueryScopeKind.Parent or QueryScopeKind.ContainingForce =>
                    includeChildren ? FlattenSelections(force.ChildSelections) : TopLevelSelections(force),
                QueryScopeKind.ContainingRoster =>
                    includeChildren ? AllSelectionsFlattened() : AllTopLevelSelections(),
                _ => includeChildren ? FlattenSelections(force.ChildSelections) : TopLevelSelections(force),
            };

            decimal sum = 0;
            foreach (var sel in selections)
            {
                if (!EntryIdMatches(sel.Declaration.EntryId, targetId)) continue;
                foreach (var cost in sel.Declaration.Costs)
                {
                    if (cost.TypeId == costTypeId)
                        sum += cost.Value;
                }
            }
            return sum;
        }

        private decimal CountTotalSelectionsInScope(IQuerySymbol query, ForceSymbol force)
        {
            bool includeChildren = query.Options.HasFlag(QueryOptions.IncludeDescendantSelections);
            var selections = query.ScopeKind switch
            {
                QueryScopeKind.Parent or QueryScopeKind.ContainingForce =>
                    includeChildren ? FlattenSelections(force.ChildSelections) : TopLevelSelections(force),
                QueryScopeKind.ContainingRoster =>
                    includeChildren ? AllSelectionsFlattened() : AllTopLevelSelections(),
                _ => includeChildren ? FlattenSelections(force.ChildSelections) : TopLevelSelections(force),
            };

            decimal count = 0;
            foreach (var sel in selections)
                count += sel.SelectedCount;
            return count;
        }

        private static IEnumerable<SelectionSymbol> TopLevelSelections(ForceSymbol force) => force.ChildSelections;

        private IEnumerable<SelectionSymbol> AllTopLevelSelections() =>
            _roster.Forces.SelectMany(f => f.ChildSelections);

        private IEnumerable<SelectionSymbol> AllSelectionsFlattened() =>
            _roster.Forces.SelectMany(f => FlattenSelections(f.ChildSelections));

        private static int CountSelectionsInForce(string entryId, ForceSymbol force)
        {
            int count = 0;
            foreach (var sel in FlattenSelections(force.ChildSelections))
            {
                if (EntryIdMatches(sel.Declaration.EntryId, entryId))
                    count += sel.SelectedCount;
            }
            return count;
        }

        private bool IsEffectivelyHidden(ISelectionEntryContainerSymbol entry, ForceSymbol force)
        {
            // Fast path: if entry has no effects (modifiers), just use the declaration value.
            // This avoids the full effective entry evaluation (name, costs, constraints, etc.)
            // which is expensive and unnecessary when we only need the hidden flag.
            if (entry.Effects.IsEmpty)
                return entry.IsHidden;

            // Slow path: evaluate modifiers that can change hidden state
            return _effectiveCache.Evaluator.GetEffectiveHidden(entry, selection: null, force);
        }

        private static IEnumerable<SelectionSymbol> FlattenSelections(ImmutableArray<SelectionSymbol> selections)
        {
            foreach (var sel in selections)
            {
                yield return sel;
                foreach (var desc in FlattenSelections(sel.ChildSelections))
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

        private void CheckConstraint(
            QueryComparisonType comparison,
            decimal constraintValue,
            decimal count,
            string entryId,
            string ownerType,
            string? ownerEntryId,
            string constraintId)
        {
            bool isMin = comparison == QueryComparisonType.GreaterThanOrEqual;
            bool isMax = comparison == QueryComparisonType.LessThanOrEqual;

            if (isMin && count < constraintValue - 0.001m)
            {
                _diagnostics.Add(ErrorCode.WRN_MinSelectionCountViolation, Location.None,
                    ownerType, ownerEntryId ?? "", entryId, constraintId);
            }
            else if (isMax && constraintValue >= 0 && count > constraintValue + 0.001m)
            {
                _diagnostics.Add(ErrorCode.WRN_MaxSelectionCountViolation, Location.None,
                    ownerType, ownerEntryId ?? "", entryId, constraintId);
            }
        }

        // ──────────────────────────────────────────────────────────────────
        //  Index building
        // ──────────────────────────────────────────────────────────────────

        private void BuildIndex()
        {
            var gs = _compilation.GlobalNamespace.RootCatalogue;
            IndexCatalogue(gs);
            foreach (var force in _roster.Forces)
            {
                IndexCatalogue(force.CatalogueReference.Catalogue);
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

        // ──────────────────────────────────────────────────────────────────
        //  EntryId path matching
        // ──────────────────────────────────────────────────────────────────

        /// <summary>
        /// Checks if a selection's entryId (which may be a "::" path like "link-1::shared-unit")
        /// matches the given <paramref name="targetId"/> (a single segment).
        /// </summary>
        private static bool EntryIdMatches(string selectionEntryId, string targetId)
        {
            if (selectionEntryId == targetId)
                return true;
            // Check if any segment of the :: path matches
            var span = selectionEntryId.AsSpan();
            while (true)
            {
                var sepIndex = span.IndexOf("::", StringComparison.Ordinal);
                if (sepIndex < 0)
                    return span.SequenceEqual(targetId.AsSpan());
                if (span[..sepIndex].SequenceEqual(targetId.AsSpan()))
                    return true;
                span = span[(sepIndex + 2)..];
            }
        }

        /// <summary>
        /// Checks if a selection's entryId (which may be a "::" path) contains any ID in the set.
        /// </summary>
        private static bool EntryIdMatchesAny(string selectionEntryId, HashSet<string> ids)
        {
            if (ids.Contains(selectionEntryId))
                return true;
            // Check if any segment of the :: path is in the set
            var span = selectionEntryId.AsSpan();
            while (true)
            {
                var sepIndex = span.IndexOf("::", StringComparison.Ordinal);
                if (sepIndex < 0)
                    return ids.Contains(span.ToString());
                if (ids.Contains(span[..sepIndex].ToString()))
                    return true;
                span = span[(sepIndex + 2)..];
            }
        }

        /// <summary>
        /// Looks up an entry in the index by any segment of a "::" path.
        /// </summary>
        private bool TryGetIndexedEntry(string entryId, [System.Diagnostics.CodeAnalysis.NotNullWhen(true)] out ISelectionEntryContainerSymbol? result)
        {
            if (_entryIndex.TryGetValue(entryId, out result))
                return true;
            // Try each segment of the :: path
            var span = entryId.AsSpan();
            while (true)
            {
                var sepIndex = span.IndexOf("::", StringComparison.Ordinal);
                if (sepIndex < 0)
                    return _entryIndex.TryGetValue(span.ToString(), out result);
                if (_entryIndex.TryGetValue(span[..sepIndex].ToString(), out result))
                    return true;
                span = span[(sepIndex + 2)..];
            }
        }

        /// <summary>
        /// Gets effective constraint values from the <see cref="_effectiveCache"/>.
        /// Returns a dictionary of constraint ID → effective ReferenceValue,
        /// extracted from the effective entry's constraints.
        /// </summary>
        private Dictionary<string, decimal> GetEffectiveConstraintValues(
            IContainerEntrySymbol entry,
            ISelectionSymbol? selection,
            IForceSymbol? force)
        {
            if (entry is ISelectionEntryContainerSymbol sec)
            {
                var effectiveEntry = _effectiveCache.GetEffectiveEntry(sec, selection, force);
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
}
