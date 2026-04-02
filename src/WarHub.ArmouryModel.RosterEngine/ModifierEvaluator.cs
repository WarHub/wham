using WarHub.ArmouryModel.Source;

namespace WarHub.ArmouryModel.RosterEngine;

/// <summary>
/// Evaluates IEffectSymbol modifiers against runtime roster state.
/// Produces effective values for entry properties (name, hidden, costs, characteristics, etc.)
/// </summary>
public sealed class ModifierEvaluator
{
    private readonly RosterNode _roster;
    private readonly Compilation _compilation;

    public ModifierEvaluator(RosterNode roster, Compilation compilation)
    {
        _roster = roster;
        _compilation = compilation;
    }

    /// <summary>
    /// Gets the effective name for a selection entry after applying modifiers.
    /// </summary>
    public string GetEffectiveName(ISelectionEntryContainerSymbol entry, SelectionNode? selection, ForceNode? force)
    {
        var name = entry.Name ?? "";
        var context = new EvalContext(selection, force, entry);
        foreach (var effect in entry.Effects)
        {
            name = ApplyNameEffect(effect, name, context);
        }
        return name;
    }

    /// <summary>
    /// Gets the effective hidden state for a selection entry after applying modifiers.
    /// </summary>
    public bool GetEffectiveHidden(IEntrySymbol entry, SelectionNode? selection, ForceNode? force)
    {
        var hidden = entry.IsHidden;
        var context = new EvalContext(selection, force, entry);
        foreach (var effect in entry.Effects)
        {
            hidden = ApplyHiddenEffect(effect, hidden, context);
        }
        return hidden;
    }

    /// <summary>
    /// Gets the effective constraint reference values after applying modifiers.
    /// Returns a dictionary of constraintId -> effective value.
    /// </summary>
    public Dictionary<string, decimal> GetEffectiveConstraintValues(
        IContainerEntrySymbol entry,
        SelectionNode? selection,
        ForceNode? force)
    {
        var values = new Dictionary<string, decimal>(StringComparer.Ordinal);
        foreach (var constraint in entry.Constraints)
        {
            if (constraint.Id is not null)
                values[constraint.Id] = constraint.Query.ReferenceValue ?? 0m;
        }
        var context = new EvalContext(selection, force, entry);
        foreach (var effect in entry.Effects)
        {
            ApplyConstraintEffect(effect, values, context);
        }
        return values;
    }

    /// <summary>
    /// Gets the effective costs for a selection entry after applying modifiers.
    /// Returns a dictionary of costTypeId -> value.
    /// </summary>
    public Dictionary<string, decimal> GetEffectiveCosts(
        ISelectionEntryContainerSymbol entry,
        SelectionNode? selection,
        ForceNode? force)
    {
        var costs = new Dictionary<string, decimal>(StringComparer.Ordinal);
        foreach (var cost in entry.Costs)
        {
            var typeId = cost.Type?.Id;
            if (typeId is not null)
                costs[typeId] = cost.Value;
        }

        var context = new EvalContext(selection, force, entry);
        foreach (var effect in entry.Effects)
        {
            ApplyCostEffect(effect, costs, context);
        }
        return costs;
    }

    /// <summary>
    /// Gets the effective value for a characteristic after applying modifiers.
    /// </summary>
    public string GetEffectiveCharacteristic(
        IEntrySymbol profileEntry,
        string characteristicTypeId,
        string currentValue,
        SelectionNode? selection,
        ForceNode? force)
    {
        var value = currentValue;
        var context = new EvalContext(selection, force, profileEntry);
        foreach (var effect in profileEntry.Effects)
        {
            value = ApplyCharacteristicEffect(effect, characteristicTypeId, value, context);
        }
        return value;
    }

    /// <summary>
    /// Gets the effective page for an entry after applying page modifiers.
    /// Returns null if no page modifiers fired and no page was set.
    /// </summary>
    public string? GetEffectivePage(IEntrySymbol entry, SelectionNode? selection, ForceNode? force)
    {
        string? page = null;
        var context = new EvalContext(selection, force, entry);
        foreach (var effect in entry.Effects)
        {
            page = ApplyPageEffect(effect, page, context);
        }
        return page;
    }

    private string? ApplyPageEffect(IEffectSymbol effect, string? page, EvalContext context)
    {
        if (effect.TargetKind == EffectTargetKind.PublicationPage && EvaluateEffectCondition(effect, context))
        {
            page = ApplyStringEffect(effect, page ?? "", context);
        }
        // Process children
        if (effect.ChildrenWhenSatisfied.Length > 0 || effect.ChildrenWhenUnsatisfied.Length > 0)
        {
            var satisfied = EvaluateCondition(effect.Condition, context);
            var children = satisfied ? effect.ChildrenWhenSatisfied : effect.ChildrenWhenUnsatisfied;
            var repeatCount = satisfied ? GetRepeatCount(effect, context) : 1;
            for (int r = 0; r < repeatCount; r++)
            {
                foreach (var child in children)
                {
                    page = ApplyPageEffect(child, page, context);
                }
            }
        }
        return page;
    }

    /// <summary>
    /// Gets the effective description for a rule entry after applying modifiers.
    /// </summary>
    public string GetEffectiveRuleDescription(
        IEntrySymbol ruleEntry,
        string currentDescription,
        SelectionNode? selection,
        ForceNode? force)
    {
        var desc = currentDescription;
        var context = new EvalContext(selection, force, ruleEntry);
        foreach (var effect in ruleEntry.Effects)
        {
            desc = ApplyRuleDescriptionEffect(effect, desc, context);
        }
        return desc;
    }

    private string ApplyRuleDescriptionEffect(IEffectSymbol effect, string desc, EvalContext context)
    {
        if (effect.TargetKind == EffectTargetKind.RuleDescription && EvaluateEffectCondition(effect, context))
        {
            desc = ApplyStringEffect(effect, desc, context);
        }
        // Process children (ModifierGroups)
        if (effect.ChildrenWhenSatisfied.Length > 0 || effect.ChildrenWhenUnsatisfied.Length > 0)
        {
            var satisfied = EvaluateCondition(effect.Condition, context);
            var children = satisfied ? effect.ChildrenWhenSatisfied : effect.ChildrenWhenUnsatisfied;
            var repeatCount = satisfied ? GetRepeatCount(effect, context) : 1;
            for (int r = 0; r < repeatCount; r++)
            {
                foreach (var child in children)
                {
                    desc = ApplyRuleDescriptionEffect(child, desc, context);
                }
            }
        }
        return desc;
    }

    /// <summary>
    /// Gets effective categories for a selection entry, applying add/remove/set-primary/unset-primary modifiers.
    /// Uses the entry's declared categories as starting point.
    /// </summary>
    public (List<string> CategoryIds, string? PrimaryCategoryId) GetEffectiveCategories(
        ISelectionEntryContainerSymbol entry,
        SelectionNode? selection,
        ForceNode? force)
    {
        var categories = new List<string>();
        string? primaryId = null;

        // Start with declared categories
        foreach (var cat in entry.Categories)
        {
            if (cat.Id is not null)
                categories.Add(cat.Id);
        }
        if (entry.PrimaryCategory?.Id is { } primId)
            primaryId = primId;

        var context = new EvalContext(selection, force, entry);
        foreach (var effect in entry.Effects)
        {
            ApplyCategoryEffect(effect, categories, ref primaryId, context);
        }
        return (categories, primaryId);
    }

    /// <summary>
    /// Gets effective categories starting from a provided initial category list.
    /// Used when runtime categories differ from declared categories (e.g., inherited from groups).
    /// </summary>
    public (List<string> CategoryIds, string? PrimaryCategoryId) GetEffectiveCategoriesFrom(
        ISelectionEntryContainerSymbol entry,
        List<string> initialCategoryIds,
        string? initialPrimaryId,
        SelectionNode? selection,
        ForceNode? force)
    {
        var categories = new List<string>(initialCategoryIds);
        string? primaryId = initialPrimaryId;

        var context = new EvalContext(selection, force, entry);
        foreach (var effect in entry.Effects)
        {
            ApplyCategoryEffect(effect, categories, ref primaryId, context);
        }
        return (categories, primaryId);
    }

    // ──────────────────────────────────────────────────────────────────
    //  Effect application
    // ──────────────────────────────────────────────────────────────────

    private string ApplyNameEffect(IEffectSymbol effect, string name, EvalContext context)
    {
        if (effect.TargetKind == EffectTargetKind.SymbolName)
        {
            name = ApplyStringEffect(effect, name, context);
        }
        // Process children
        if (effect.ChildrenWhenSatisfied.Length > 0 || effect.ChildrenWhenUnsatisfied.Length > 0)
        {
            var satisfied = EvaluateCondition(effect.Condition, context);
            var children = satisfied ? effect.ChildrenWhenSatisfied : effect.ChildrenWhenUnsatisfied;
            var repeatCount = satisfied ? GetRepeatCount(effect, context) : 1;
            for (int r = 0; r < repeatCount; r++)
            {
                foreach (var child in children)
                {
                    name = ApplyNameEffect(child, name, context);
                }
            }
        }
        return name;
    }

    private bool ApplyHiddenEffect(IEffectSymbol effect, bool hidden, EvalContext context)
    {
        if (effect.TargetKind == EffectTargetKind.EntryHiddenState &&
            effect.FunctionKind == EffectOperation.SetValue)
        {
            if (EvaluateEffectCondition(effect, context))
            {
                hidden = ParseBool(effect.OperandValue);
            }
        }
        // Process children
        if (effect.ChildrenWhenSatisfied.Length > 0 || effect.ChildrenWhenUnsatisfied.Length > 0)
        {
            var satisfied = EvaluateCondition(effect.Condition, context);
            var children = satisfied ? effect.ChildrenWhenSatisfied : effect.ChildrenWhenUnsatisfied;
            foreach (var child in children)
            {
                hidden = ApplyHiddenEffect(child, hidden, context);
            }
        }
        return hidden;
    }

    private void ApplyConstraintEffect(
        IEffectSymbol effect, Dictionary<string, decimal> constraintValues, EvalContext context)
    {
        if (effect.TargetKind == EffectTargetKind.Member && effect.TargetMember is IConstraintSymbol targetConstraint)
        {
            var constraintId = targetConstraint.Id;
            if (constraintId is not null && constraintValues.ContainsKey(constraintId))
            {
                if (EvaluateEffectCondition(effect, context))
                {
                    var repeatCount = GetRepeatCount(effect, context);
                    var current = constraintValues[constraintId];
                    for (int r = 0; r < repeatCount; r++)
                    {
                        current = ApplyNumericOperation(effect.FunctionKind, current, ParseDecimal(effect.OperandValue));
                    }
                    constraintValues[constraintId] = current;
                }
            }
        }
        if (effect.ChildrenWhenSatisfied.Length > 0 || effect.ChildrenWhenUnsatisfied.Length > 0)
        {
            var satisfied = EvaluateCondition(effect.Condition, context);
            var children = satisfied ? effect.ChildrenWhenSatisfied : effect.ChildrenWhenUnsatisfied;
            var repeatCount = satisfied ? GetRepeatCount(effect, context) : 1;
            for (int r = 0; r < repeatCount; r++)
            {
                foreach (var child in children)
                {
                    ApplyConstraintEffect(child, constraintValues, context);
                }
            }
        }
    }

    /// <summary>
    /// Resolves the resource definition type ID from an effect's TargetMember.
    /// The TargetMember can be an IResourceDefinitionSymbol directly (cost type / profile type)
    /// or an IResourceEntrySymbol (cost / characteristic) whose .Type gives the definition.
    /// </summary>
    private static string? ResolveResourceTypeId(ISymbol? targetMember) => targetMember switch
    {
        IResourceDefinitionSymbol def => def.Id,
        IResourceEntrySymbol entry => entry.Type?.Id,
        _ => null
    };

    private void ApplyCostEffect(IEffectSymbol effect, Dictionary<string, decimal> costs, EvalContext context)
    {
        if (effect.TargetKind == EffectTargetKind.Member && ResolveResourceTypeId(effect.TargetMember) is { } typeId)
        {
            if (EvaluateEffectCondition(effect, context))
            {
                var repeatCount = GetRepeatCount(effect, context);
                costs.TryGetValue(typeId, out var current);
                for (int r = 0; r < repeatCount; r++)
                {
                    current = ApplyNumericOperation(effect.FunctionKind, current, ParseDecimal(effect.OperandValue));
                }
                costs[typeId] = current;
            }
        }
        // Process children
        if (effect.ChildrenWhenSatisfied.Length > 0 || effect.ChildrenWhenUnsatisfied.Length > 0)
        {
            var satisfied = EvaluateCondition(effect.Condition, context);
            var children = satisfied ? effect.ChildrenWhenSatisfied : effect.ChildrenWhenUnsatisfied;
            var repeatCount = satisfied ? GetRepeatCount(effect, context) : 1;
            for (int r = 0; r < repeatCount; r++)
            {
                foreach (var child in children)
                {
                    ApplyCostEffect(child, costs, context);
                }
            }
        }
    }

    private string ApplyCharacteristicEffect(
        IEffectSymbol effect, string characteristicTypeId, string value, EvalContext context)
    {
        if (effect.TargetKind == EffectTargetKind.Member &&
            ResolveResourceTypeId(effect.TargetMember) == characteristicTypeId)
        {
            value = ApplyStringEffect(effect, value, context);
        }
        // Process children
        if (effect.ChildrenWhenSatisfied.Length > 0 || effect.ChildrenWhenUnsatisfied.Length > 0)
        {
            var satisfied = EvaluateCondition(effect.Condition, context);
            var children = satisfied ? effect.ChildrenWhenSatisfied : effect.ChildrenWhenUnsatisfied;
            var repeatCount = satisfied ? GetRepeatCount(effect, context) : 1;
            for (int r = 0; r < repeatCount; r++)
            {
                foreach (var child in children)
                {
                    value = ApplyCharacteristicEffect(child, characteristicTypeId, value, context);
                }
            }
        }
        return value;
    }

    private void ApplyCategoryEffect(
        IEffectSymbol effect, List<string> categories, ref string? primaryId, EvalContext context)
    {
        if (effect.TargetKind == EffectTargetKind.EntryCategory && EvaluateEffectCondition(effect, context))
        {
            var catId = (effect.OperandSymbol as ICategoryEntrySymbol)?.Id ?? effect.OperandValue;
            if (catId is not null)
            {
                switch (effect.FunctionKind)
                {
                    case EffectOperation.SetCategoryPrimary:
                        primaryId = catId;
                        if (!categories.Contains(catId))
                            categories.Add(catId);
                        break;
                    case EffectOperation.UnsetCategoryPrimary:
                        if (primaryId == catId)
                            primaryId = null;
                        break;
                    case EffectOperation.AddCategory:
                        if (!categories.Contains(catId))
                            categories.Add(catId);
                        break;
                    case EffectOperation.RemoveCategory:
                        categories.Remove(catId);
                        break;
                }
            }
        }
        // Process children
        if (effect.ChildrenWhenSatisfied.Length > 0 || effect.ChildrenWhenUnsatisfied.Length > 0)
        {
            var satisfied = EvaluateCondition(effect.Condition, context);
            var children = satisfied ? effect.ChildrenWhenSatisfied : effect.ChildrenWhenUnsatisfied;
            foreach (var child in children)
            {
                ApplyCategoryEffect(child, categories, ref primaryId, context);
            }
        }
    }

    private string ApplyStringEffect(IEffectSymbol effect, string current, EvalContext context)
    {
        if (!EvaluateEffectCondition(effect, context))
            return current;

        var repeatCount = GetRepeatCount(effect, context);
        for (int r = 0; r < repeatCount; r++)
        {
            current = effect.FunctionKind switch
            {
                EffectOperation.SetValue => effect.OperandValue ?? "",
                EffectOperation.AppendText => current + " " + (effect.OperandValue ?? ""),
                EffectOperation.IncrementValue => IncrementString(current, effect.OperandValue),
                EffectOperation.DecrementValue => DecrementString(current, effect.OperandValue),
                _ => current,
            };
        }
        return current;
    }

    // ──────────────────────────────────────────────────────────────────
    //  Condition evaluation
    // ──────────────────────────────────────────────────────────────────

    private bool EvaluateEffectCondition(IEffectSymbol effect, EvalContext context)
    {
        if (effect.Condition is null)
            return true;
        return EvaluateCondition(effect.Condition, context);
    }

    private bool EvaluateCondition(IConditionSymbol? condition, EvalContext context)
    {
        if (condition is null)
            return true;

        // Evaluate own query
        bool queryResult = true;
        if (condition.Query is { } query)
        {
            queryResult = EvaluateQuery(query, context);
        }

        // Evaluate children with logical operator
        bool childrenResult = EvaluateChildConditions(condition.Children, condition.ChildrenOperator, context);

        return queryResult && childrenResult;
    }

    private bool EvaluateChildConditions(
        ImmutableArray<IConditionSymbol> children,
        LogicalOperator op,
        EvalContext context)
    {
        if (children.Length == 0)
            return true; // No children → vacuously true

        return op switch
        {
            LogicalOperator.Conjunction or LogicalOperator.Identity =>
                children.All(c => EvaluateCondition(c, context)),
            LogicalOperator.Disjunction =>
                children.Any(c => EvaluateCondition(c, context)),
            LogicalOperator.Negation when children.Length == 1 =>
                !EvaluateCondition(children[0], context),
            _ => children.All(c => EvaluateCondition(c, context)),
        };
    }

    // ──────────────────────────────────────────────────────────────────
    //  Query evaluation
    // ──────────────────────────────────────────────────────────────────

    private bool EvaluateQuery(IQuerySymbol query, EvalContext context)
    {
        // Handle special comparison types first
        if (query.Comparison == QueryComparisonType.InstanceOf)
            return EvaluateInstanceOf(query, context);
        if (query.Comparison == QueryComparisonType.NotInstanceOf)
            return !EvaluateInstanceOf(query, context);

        // BattleScribe behavior: scope=self with field=selections always returns count 0
        // (the current selection doesn't count itself for non-instanceOf conditions)
        if (query.ScopeKind == QueryScopeKind.Self && query.ValueKind == QueryValueKind.SelectionCount)
        {
            var referenceValue0 = query.ReferenceValue ?? 0m;
            return CompareValues(query.Comparison, 0m, referenceValue0);
        }

        // BattleScribe behavior: null/empty childId returns NaN → condition false
        // Exception: scope=ancestor still works with null childId (counts self entry)
        if (query.ValueFilterKind == QueryFilterKind.Unknown
            && query.ScopeKind != QueryScopeKind.ContainingAncestor)
        {
            return false;
        }

        // Calculate query value
        var resultValue = CalculateQueryValue(query, context);

        var referenceValue = query.ReferenceValue ?? 0m;
        // Apply percent if needed: threshold = totalInScope * value / 100
        if (query.Options.HasFlag(QueryOptions.ValuePercentage))
        {
            var totalInScope = CountTotalSelectionsInScope(query, context);
            referenceValue = totalInScope * referenceValue / 100m;
            if (query.Options.HasFlag(QueryOptions.ValueRoundUp))
                referenceValue = Math.Ceiling(referenceValue);
        }

        return CompareValues(query.Comparison, resultValue, referenceValue);
    }

    private bool EvaluateInstanceOf(IQuerySymbol query, EvalContext context)
    {
        // "instanceOf" is a boolean type check, NOT a count.
        // It checks if the current selection (or ancestor) matches a specific entry/category.
        // Only scope=self and scope=ancestor work; all other scopes return false
        // (force, roster, primary-category etc. are not selection entries).

        if (query.ScopeKind == QueryScopeKind.Self)
        {
            return CheckInstanceOf(query, context.Selection);
        }

        if (query.ScopeKind == QueryScopeKind.ContainingAncestor)
        {
            return CheckAncestorInstanceOf(query, context);
        }

        // Non-self, non-ancestor scopes: the scope element (force, roster, etc.)
        // is never an instance of a selection entry, so instanceOf always returns false.
        return false;
    }

    private static bool CheckInstanceOf(IQuerySymbol query, SelectionNode? selection)
    {
        if (selection is null)
            return false;

        // Check by entry type (unit/model/upgrade) — childId was "unit"/"model"/"upgrade"
        if (query.ValueFilterKind is QueryFilterKind.UnitEntry)
            return selection.Type == SelectionEntryKind.Unit;
        if (query.ValueFilterKind is QueryFilterKind.ModelEntry)
            return selection.Type == SelectionEntryKind.Model;
        if (query.ValueFilterKind is QueryFilterKind.UpgradeEntry)
            return selection.Type == SelectionEntryKind.Upgrade;

        // Check by specific entry/category ID
        var filterId = query.FilterSymbol?.Id;
        if (filterId is null)
            return false;

        // Check by entry ID
        if (selection.EntryId == filterId)
            return true;

        // Check by category
        foreach (var cat in selection.Categories)
        {
            if (cat.EntryId == filterId)
                return true;
        }

        return false;
    }

    private bool CheckAncestorInstanceOf(IQuerySymbol query, EvalContext context)
    {
        // Walk up parent chain: check if any ancestor matches
        // Find parent selection from force's selection tree
        if (context.Selection is not null && context.Force is not null)
        {
            var parent = FindParentSelection(context.Force, context.Selection);
            while (parent is not null)
            {
                if (CheckInstanceOf(query, parent))
                    return true;
                parent = FindParentSelection(context.Force, parent);
            }
        }
        return false;
    }

    private static SelectionNode? FindParentSelection(ForceNode force, SelectionNode child)
    {
        foreach (var rootSel in force.Selections)
        {
            if (rootSel == child) return null; // root-level, parent is force
            var found = FindParentInTree(rootSel, child);
            if (found is not null) return found;
        }
        return null;
    }

    private static SelectionNode? FindParentInTree(SelectionNode parent, SelectionNode target)
    {
        foreach (var child in parent.Selections)
        {
            if (child == target) return parent;
            var found = FindParentInTree(child, target);
            if (found is not null) return found;
        }
        return null;
    }

    private decimal CalculateQueryValue(IQuerySymbol query, EvalContext context)
    {
        // Get the scope to count in
        var selections = GetSelectionsInScope(query, context);

        return query.ValueKind switch
        {
            QueryValueKind.SelectionCount => CountSelections(selections, query),
            QueryValueKind.ForceCount => CountForces(query, context),
            QueryValueKind.MemberValue => SumMemberValues(selections, query),
            QueryValueKind.MemberValueLimit => GetMemberValueLimit(query),
            _ => 0m,
        };
    }

    private IEnumerable<SelectionNode> GetSelectionsInScope(IQuerySymbol query, EvalContext context)
    {
        return query.ScopeKind switch
        {
            QueryScopeKind.Self => GetSelfSelections(context),
            QueryScopeKind.Parent => GetParentSelections(context),
            QueryScopeKind.ContainingForce => GetForceSelections(context),
            QueryScopeKind.ContainingRoster => GetRosterSelections(),
            QueryScopeKind.ContainingAncestor => GetAncestorSelections(query, context),
            QueryScopeKind.ReferencedEntry => GetReferencedEntrySelections(query),
            QueryScopeKind.PrimaryCategory => GetPrimaryCategorySelections(context),
            _ => [],
        };
    }

    private IEnumerable<SelectionNode> GetSelfSelections(EvalContext context)
    {
        if (context.Selection is { } sel)
            yield return sel;
    }

    private IEnumerable<SelectionNode> GetParentSelections(EvalContext context)
    {
        // When no specific selection context but force exists,
        // "parent" of a root entry is the force — return force-level selections
        if (context.Selection is null)
        {
            if (context.Force is not null)
            {
                foreach (var s in context.Force.Selections)
                    yield return s;
            }
            yield break;
        }

        // Find the parent selection in the roster tree
        if (context.Force is not null)
        {
            foreach (var rootSel in context.Force.Selections)
            {
                if (rootSel == context.Selection)
                {
                    // Root selection's parent is the force — return force-level selections
                    foreach (var s in context.Force.Selections)
                        yield return s;
                    yield break;
                }
                var parent = FindParentSelection(rootSel, context.Selection);
                if (parent is not null)
                {
                    // Return the parent's children (siblings of current selection)
                    foreach (var s in parent.Selections)
                        yield return s;
                    yield break;
                }
            }
        }
    }

    private static SelectionNode? FindParentSelection(SelectionNode current, SelectionNode target)
    {
        foreach (var child in current.Selections)
        {
            if (child == target)
                return current;
            var found = FindParentSelection(child, target);
            if (found is not null)
                return found;
        }
        return null;
    }

    private IEnumerable<SelectionNode> GetForceSelections(EvalContext context)
    {
        if (context.Force is null)
            yield break;

        foreach (var sel in context.Force.Selections)
        {
            yield return sel;
            foreach (var desc in GetDescendantSelections(sel))
                yield return desc;
        }
    }

    private IEnumerable<SelectionNode> GetRosterSelections()
    {
        foreach (var force in _roster.Forces)
        {
            foreach (var sel in force.Selections)
            {
                yield return sel;
                foreach (var desc in GetDescendantSelections(sel))
                    yield return desc;
            }
        }
    }

    private IEnumerable<SelectionNode> GetAncestorSelections(IQuerySymbol query, EvalContext context)
    {
        // Ancestor scope: walk up from current selection to find matching ancestor
        if (context.Selection is null || context.Force is null)
            yield break;

        var scopeId = query.ScopeSymbol?.Id;
        if (scopeId is null)
        {
            // No specific ancestor — return roster-level selections
            foreach (var sel in GetRosterSelections())
                yield return sel;
            yield break;
        }

        // Find ancestor matching the scope symbol
        foreach (var rootSel in context.Force.Selections)
        {
            var path = FindPathToSelection(rootSel, context.Selection);
            if (path is not null)
            {
                // Walk up the path looking for matching entry
                foreach (var ancestor in path)
                {
                    if (ancestor.EntryId == scopeId || ancestor.EntryGroupId == scopeId)
                    {
                        yield return ancestor;
                        foreach (var desc in GetDescendantSelections(ancestor))
                            yield return desc;
                        yield break;
                    }
                }
            }
        }
    }

    private IEnumerable<SelectionNode> GetReferencedEntrySelections(IQuerySymbol query)
    {
        // Scope is a specific entry — find all selections matching that entry across the roster
        var entryId = query.ScopeSymbol?.Id;
        if (entryId is null)
            yield break;

        foreach (var sel in GetRosterSelections())
        {
            if (sel.EntryId == entryId || sel.EntryGroupId == entryId)
                yield return sel;
        }
    }

    private IEnumerable<SelectionNode> GetPrimaryCategorySelections(EvalContext context)
    {
        // Find the primary category of the current selection's root,
        // then find all selections in the force with that category
        if (context.Selection is null || context.Force is null)
            yield break;

        var primaryCat = context.Selection.Categories
            .FirstOrDefault(c => c.Primary)?.EntryId;

        if (primaryCat is null)
            yield break;

        foreach (var sel in GetForceSelections(context))
        {
            if (SelectionHasCategory(sel, primaryCat))
                yield return sel;
        }
    }

    private static IEnumerable<SelectionNode> GetDescendantSelections(SelectionNode sel)
    {
        foreach (var child in sel.Selections)
        {
            yield return child;
            foreach (var desc in GetDescendantSelections(child))
                yield return desc;
        }
    }

    private decimal CountSelections(IEnumerable<SelectionNode> selections, IQuerySymbol query)
    {
        decimal count = 0;
        foreach (var sel in selections)
        {
            if (MatchesFilter(sel, query))
            {
                count += sel.Number;
            }
        }
        return count;
    }

    private decimal CountTotalSelectionsInScope(IQuerySymbol query, EvalContext context)
    {
        // Count ALL selections in scope (no filter applied) for percentValue threshold
        var selections = GetSelectionsInScope(query, context);
        decimal count = 0;
        foreach (var sel in selections)
        {
            count += sel.Number;
        }
        return count;
    }

    private decimal CountForces(IQuerySymbol query, EvalContext context)
    {
        decimal count = 0;
        var filterId = query.FilterSymbol?.Id;

        foreach (var force in _roster.Forces)
        {
            if (filterId is null || force.EntryId == filterId)
                count++;
        }
        return count;
    }

    private decimal SumMemberValues(IEnumerable<SelectionNode> selections, IQuerySymbol query)
    {
        var valueTypeId = query.ValueTypeSymbol?.Id;
        if (valueTypeId is null) return 0m;

        decimal sum = 0;
        foreach (var sel in selections)
        {
            if (MatchesFilter(sel, query))
            {
                foreach (var cost in sel.Costs)
                {
                    if (cost.TypeId == valueTypeId)
                    {
                        sum += cost.Value;
                    }
                }
            }
        }
        return sum;
    }

    private decimal GetMemberValueLimit(IQuerySymbol query)
    {
        var valueTypeId = query.ValueTypeSymbol?.Id;
        if (valueTypeId is null) return 0m;

        foreach (var limit in _roster.CostLimits)
        {
            if (limit.TypeId == valueTypeId)
                return limit.Value;
        }
        return -1m; // No limit
    }

    private static bool MatchesFilter(SelectionNode sel, IQuerySymbol query)
    {
        return query.ValueFilterKind switch
        {
            QueryFilterKind.Anything => true,
            QueryFilterKind.Unknown => true,
            QueryFilterKind.UnitEntry => sel.Type == SelectionEntryKind.Unit,
            QueryFilterKind.ModelEntry => sel.Type == SelectionEntryKind.Model,
            QueryFilterKind.UpgradeEntry => sel.Type == SelectionEntryKind.Upgrade,
            QueryFilterKind.SpecifiedEntry => MatchesSpecifiedEntry(sel, query),
            _ => true,
        };
    }

    private static bool MatchesSpecifiedEntry(SelectionNode sel, IQuerySymbol query)
    {
        var filterSymbol = query.FilterSymbol;
        // Error symbols (binding failures) or null → don't match anything
        if (filterSymbol is null || filterSymbol.Kind == SymbolKind.Error)
            return false;
        var filterId = filterSymbol.Id;
        if (filterId is null)
            return false;

        // Check if selection's entry matches
        if (sel.EntryId == filterId || sel.EntryGroupId == filterId)
            return true;

        // Check if selection has a category matching the filter
        if (SelectionHasCategory(sel, filterId))
            return true;

        return false;
    }

    private static bool MatchesEntry(SelectionNode sel, string? entryId)
    {
        if (entryId is null) return false;
        return sel.EntryId == entryId || sel.EntryGroupId == entryId;
    }

    private static bool SelectionHasCategory(SelectionNode sel, string? categoryId)
    {
        if (categoryId is null) return false;
        foreach (var cat in sel.Categories)
        {
            if (cat.EntryId == categoryId)
                return true;
        }
        return false;
    }

    private static bool MatchesEntryType(SelectionNode sel, QueryFilterKind filterKind)
    {
        return filterKind switch
        {
            QueryFilterKind.UnitEntry => sel.Type == SelectionEntryKind.Unit,
            QueryFilterKind.ModelEntry => sel.Type == SelectionEntryKind.Model,
            QueryFilterKind.UpgradeEntry => sel.Type == SelectionEntryKind.Upgrade,
            _ => false,
        };
    }

    // ──────────────────────────────────────────────────────────────────
    //  Repeat calculation
    // ──────────────────────────────────────────────────────────────────

    private int GetRepeatCount(IEffectSymbol effect, EvalContext context)
    {
        // Check direct RepetitionQuery (used by RepeatEffectSymbol)
        if (effect.RepetitionQuery is { } query)
        {
            var referenceValue = query.ReferenceValue ?? 1m;
            // Division by zero guard: value=0 means skip the repeat entirely
            if (referenceValue == 0)
                return 0;

            var value = CalculateQueryValue(query, context);
            var roundUp = query.Options.HasFlag(QueryOptions.ValueRoundUp);

            var repeats = value / referenceValue;
            var repeatCount = roundUp ? (int)Math.Ceiling(repeats) : (int)Math.Floor(repeats);
            return Math.Max(0, repeatCount * Math.Max(1, effect.Repetitions));
        }

        // Check repeat children (ModifierEffectBaseSymbol.Effects → RepeatEffectSymbol)
        if (effect.Effects.Length > 0)
        {
            int totalRepeats = 1;
            foreach (var repeatEffect in effect.Effects)
            {
                var childRepeat = GetRepeatCount(repeatEffect, context);
                if (childRepeat == 0)
                    return 0; // Any zero repeat means skip entirely
                totalRepeats = childRepeat;
            }
            return totalRepeats;
        }

        return 1;
    }

    // ──────────────────────────────────────────────────────────────────
    //  Path finding utilities
    // ──────────────────────────────────────────────────────────────────

    private static List<SelectionNode>? FindPathToSelection(SelectionNode current, SelectionNode target)
    {
        if (current == target)
            return [current];

        foreach (var child in current.Selections)
        {
            var path = FindPathToSelection(child, target);
            if (path is not null)
            {
                path.Insert(0, current);
                return path;
            }
        }
        return null;
    }

    // ──────────────────────────────────────────────────────────────────
    //  Comparison and parsing utilities
    // ──────────────────────────────────────────────────────────────────

    private static bool CompareValues(QueryComparisonType comparison, decimal actual, decimal reference)
    {
        return comparison switch
        {
            QueryComparisonType.Equal => actual == reference,
            QueryComparisonType.NotEqual => actual != reference,
            QueryComparisonType.LessThan => actual < reference,
            QueryComparisonType.LessThanOrEqual => actual <= reference,
            QueryComparisonType.GreaterThan => actual > reference,
            QueryComparisonType.GreaterThanOrEqual => actual >= reference,
            _ => false,
        };
    }

    private static decimal ApplyNumericOperation(EffectOperation op, decimal current, decimal operand)
    {
        return op switch
        {
            EffectOperation.SetValue => operand,
            EffectOperation.IncrementValue => current + operand,
            EffectOperation.DecrementValue => current - operand,
            _ => current,
        };
    }

    private static string IncrementString(string current, string? operand)
    {
        if (decimal.TryParse(current, System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture, out var currentNum) &&
            decimal.TryParse(operand, System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture, out var operandNum))
        {
            return FormatNumericValue((double)(currentNum + operandNum));
        }
        return current;
    }

    private static string DecrementString(string current, string? operand)
    {
        if (decimal.TryParse(current, System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture, out var currentNum) &&
            decimal.TryParse(operand, System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture, out var operandNum))
        {
            return FormatNumericValue((double)(currentNum - operandNum));
        }
        return current;
    }

    private static string FormatNumericValue(double value)
    {
        if (value == Math.Floor(value) && !double.IsInfinity(value))
            return ((long)value).ToString();
        return value.ToString(System.Globalization.CultureInfo.InvariantCulture);
    }

    private static bool ParseBool(string? value) =>
        string.Equals(value, "true", StringComparison.OrdinalIgnoreCase);

    private static decimal ParseDecimal(string? value) =>
        decimal.TryParse(value, System.Globalization.NumberStyles.Any,
            System.Globalization.CultureInfo.InvariantCulture, out var result) ? result : 0m;

    /// <summary>
    /// Evaluation context: runtime information about where the evaluation is happening.
    /// </summary>
    private record struct EvalContext(
        SelectionNode? Selection,
        ForceNode? Force,
        ISymbol EntrySymbol);
}
