using BattleScribeSpec;
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
    private readonly Dictionary<string, ISelectionEntryContainerSymbol> _entryIndex;
    private readonly Dictionary<string, ICategoryEntrySymbol> _categoryIndex;
    private readonly Dictionary<string, IForceEntrySymbol> _forceEntryIndex;

    private ConstraintValidator(
        RosterNode roster,
        Compilation compilation,
        IReadOnlyList<ICatalogueSymbol> forceCatalogues)
    {
        _roster = roster;
        _compilation = compilation;
        _forceCatalogues = forceCatalogues;
        _entryIndex = new Dictionary<string, ISelectionEntryContainerSymbol>(StringComparer.Ordinal);
        _categoryIndex = new Dictionary<string, ICategoryEntrySymbol>(StringComparer.Ordinal);
        _forceEntryIndex = new Dictionary<string, IForceEntrySymbol>(StringComparer.Ordinal);
        BuildIndex();
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
            var entryId = GetTargetEntryId(entry);
            if (entryId is null) continue;

            var constraints = entry.Constraints;
            if (constraints.IsEmpty) continue;

            bool hasCategoryLinks = !entry.Categories.IsEmpty;
            if (hasCategoryLinks && entry.ReferencedEntry is { } referenced && entry.Categories.IsEmpty)
                hasCategoryLinks = !referenced.Categories.IsEmpty;

            foreach (var constraint in constraints)
            {
                var query = constraint.Query;
                var constraintId = constraint.Id ?? "";

                // field=forces on selection entry: validate with roster owner
                if (query.ValueKind == QueryValueKind.ForceCount)
                {
                    decimal forceCount = query.ScopeKind == QueryScopeKind.ContainingRoster
                        ? _roster.Forces.Count
                        : 0;
                    CheckConstraint(query.Comparison, query.ReferenceValue ?? 0m, forceCount,
                        entryId, "roster", null, constraintId, errors);
                    continue;
                }

                // Shared constraint: validate once per (constraintId, entryId)
                if (query.Options.HasFlag(QueryOptions.SharedConstraint))
                {
                    if (!sharedChecked.Add((constraintId, entryId))) continue;
                }

                // Count selections or costs in scope
                decimal count;
                if (query.ValueKind == QueryValueKind.SelectionCount)
                {
                    count = CountSelectionsInScope(query, entryId, force);
                }
                else if (query.ValueKind == QueryValueKind.MemberValue)
                {
                    count = CountCostInScope(query, entryId, force);
                }
                else
                {
                    continue;
                }

                var constraintValue = query.ReferenceValue ?? 0m;

                // Percent value support
                if (query.Options.HasFlag(QueryOptions.ValuePercentage))
                {
                    var total = CountTotalSelectionsInScope(query, force);
                    constraintValue = total * constraintValue / 100m;
                    if (query.Options.HasFlag(QueryOptions.ValueRoundUp))
                        constraintValue = Math.Ceiling(constraintValue);
                }

                var (ownerType, ownerEntryId) = GetOwnerForConstraint(
                    query.Comparison, query.ScopeKind, entry, entryId);

                CheckConstraint(query.Comparison, constraintValue, count,
                    entryId, ownerType, ownerEntryId, constraintId, errors);
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

            foreach (var constraint in childEntry.Constraints)
            {
                var query = constraint.Query;
                if (query.ValueKind != QueryValueKind.SelectionCount) continue;
                if (query.ScopeKind != QueryScopeKind.Parent) continue;

                var constraintValue = query.ReferenceValue ?? 0m;
                var count = childCounts.GetValueOrDefault(child.EntryId);

                var parentEntryId = parent.EntryId;
                CheckConstraint(query.Comparison, constraintValue, count,
                    child.EntryId, "selection", parentEntryId,
                    constraint.Id ?? "", errors);
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
                foreach (var constraint in targetEntry.Constraints)
                {
                    var query = constraint.Query;
                    if (query.ValueKind != QueryValueKind.SelectionCount) continue;
                    if (query.ScopeKind != QueryScopeKind.Parent) continue;

                    var constraintValue = query.ReferenceValue ?? 0m;
                    CheckConstraint(query.Comparison, constraintValue, 0,
                        childId, "selection", parent.EntryId,
                        constraint.Id ?? "", errors);
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
            _entryIndex.TryAdd(entry.Id, entry);

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
