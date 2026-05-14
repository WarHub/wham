using WarHub.ArmouryModel.Source; // SelectionEntryKind enum

namespace WarHub.ArmouryModel.Concrete;

/// <summary>
/// Evaluates IEffectSymbol modifiers against runtime roster state.
/// Produces effective values for entry properties (name, hidden, costs, characteristics, etc.)
/// <para>
/// This evaluator runs during compilation's EffectiveEntries phase.
/// All referenced catalogue symbols are fully bound before roster evaluation begins
/// (ensured by <c>EnsureReferencedCataloguesComplete</c>), so both catalogue-level and
/// roster-level symbol properties are safe to access through their public API.
/// </para>
/// <para>
/// <b>Binding order invariant:</b> The compilation pipeline guarantees that catalogue
/// compilation completes all phases up to CheckReferences before roster compilation starts.
/// This means:
/// <list type="bullet">
///   <item>It IS safe to access <c>IEntrySymbol.ReferencedEntry</c> on catalogue symbols (already resolved)</item>
///   <item>It IS safe to navigate shared entry chains (infoLinks, entryLinks, categoryLinks)</item>
///   <item>It IS safe to read modifier condition references to other catalogue entries</item>
///   <item>It is NOT safe to access roster-level symbols that may not have completed their
///     own binding — use <c>Declaration</c> node properties instead (see docs/roster-engine.md)</item>
/// </list>
/// </para>
/// </summary>
internal sealed class ModifierEvaluator
{
    private readonly IRosterSymbol _roster;
    private readonly WhamCompilation _compilation;

    public ModifierEvaluator(IRosterSymbol roster, WhamCompilation compilation)
    {
        _roster = roster;
        _compilation = compilation;
    }

    /// <summary>
    /// Gets the effective name for an entry after applying modifiers.
    /// </summary>
    public string GetEffectiveName(IEntrySymbol entry, ISelectionSymbol? selection, IForceSymbol? force)
    {
        var name = entry.Name ?? "";
        if (entry.Effects.IsEmpty)
            return name;
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
    public bool GetEffectiveHidden(IEntrySymbol entry, ISelectionSymbol? selection, IForceSymbol? force)
    {
        var hidden = entry.IsHidden;
        if (entry.Effects.IsEmpty)
            return hidden;
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
        ISelectionSymbol? selection,
        IForceSymbol? force)
    {
        var values = new Dictionary<string, decimal>(StringComparer.Ordinal);
        foreach (var constraint in entry.Constraints)
        {
            if (constraint.Id is not null)
                values[constraint.Id] = constraint.Query.ReferenceValue ?? 0m;
        }
        if (entry.Effects.IsEmpty)
            return values;
        var context = new EvalContext(selection, force, entry);
        foreach (var effect in entry.Effects)
        {
            ApplyConstraintEffect(effect, values, context);
        }
        return values;
    }

    /// <summary>
    /// Gets the effective value of a single cost after applying modifiers.
    /// </summary>
    public decimal GetEffectiveCostValue(
        ICostSymbol cost,
        IEntrySymbol entry,
        ISelectionSymbol? selection,
        IForceSymbol? force)
    {
        var typeId = cost.Type?.Id;
        if (typeId is null || entry.Effects.IsEmpty)
            return cost.Value;
        var value = cost.Value;
        var context = new EvalContext(selection, force, entry);
        foreach (var effect in entry.Effects)
        {
            value = ApplyCostEffect(effect, typeId, value, context);
        }
        return value;
    }

    /// <summary>
    /// Gets the effective value for a characteristic after applying modifiers.
    /// </summary>
    public string GetEffectiveCharacteristic(
        IEntrySymbol profileEntry,
        string characteristicTypeId,
        string currentValue,
        ISelectionSymbol? selection,
        IForceSymbol? force)
    {
        if (profileEntry.Effects.IsEmpty)
            return currentValue;
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
    public string? GetEffectivePage(IEntrySymbol entry, ISelectionSymbol? selection, IForceSymbol? force)
    {
        if (entry.Effects.IsEmpty)
            return null;
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
        ISelectionSymbol? selection,
        IForceSymbol? force)
    {
        if (ruleEntry.Effects.IsEmpty)
            return currentDescription;
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
        ISelectionSymbol? selection,
        IForceSymbol? force)
    {
        var categories = new List<string>();
        string? primaryId = null;

        // Start with declared categories, using resolved target IDs
        foreach (var cat in entry.Categories)
        {
            var effectiveId = cat.ReferencedEntry?.Id ?? cat.Id;
            if (effectiveId is not null)
                categories.Add(effectiveId);
        }
        var primarySym = entry.PrimaryCategory;
        if (primarySym is not null)
            primaryId = primarySym.ReferencedEntry?.Id ?? primarySym.Id;

        if (!entry.Effects.IsEmpty)
        {
            var context = new EvalContext(selection, force, entry);
            foreach (var effect in entry.Effects)
            {
                ApplyEntryCategoryMutation(effect, categories, ref primaryId, context);
            }
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
        ISelectionSymbol? selection,
        IForceSymbol? force)
    {
        var categories = new List<string>(initialCategoryIds);
        string? primaryId = initialPrimaryId;

        if (!entry.Effects.IsEmpty)
        {
            var context = new EvalContext(selection, force, entry);
            foreach (var effect in entry.Effects)
            {
                ApplyEntryCategoryMutation(effect, categories, ref primaryId, context);
            }
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

    private decimal ApplyCostEffect(IEffectSymbol effect, string targetTypeId, decimal value, EvalContext context)
    {
        if (effect.TargetKind == EffectTargetKind.Member)
        {
            var resolvedId = ResolveResourceTypeId(effect.TargetMember);
            if (resolvedId == targetTypeId && EvaluateEffectCondition(effect, context))
            {
                var repeatCount = GetRepeatCount(effect, context);
                for (int r = 0; r < repeatCount; r++)
                {
                    value = ApplyNumericOperation(effect.FunctionKind, value, ParseDecimal(effect.OperandValue));
                }
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
                    value = ApplyCostEffect(child, targetTypeId, value, context);
                }
            }
        }
        return value;
    }

    private string ApplyCharacteristicEffect(
        IEffectSymbol effect, string characteristicTypeId, string value, EvalContext context)
    {
        if (effect.TargetKind == EffectTargetKind.Member)
        {
            var resolvedId = ResolveResourceTypeId(effect.TargetMember);
            if (resolvedId == characteristicTypeId)
            {
                value = ApplyStringEffect(effect, value, context);
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
                    value = ApplyCharacteristicEffect(child, characteristicTypeId, value, context);
                }
            }
        }
        return value;
    }

    /// <summary>
    /// Mutates the entry's category set: add/remove categories, set/unset primary.
    /// These are entry-level modifiers with <c>field="category"</c>.
    /// </summary>
    private void ApplyEntryCategoryMutation(
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
                ApplyEntryCategoryMutation(child, categories, ref primaryId, context);
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

        if (query.ScopeKind == QueryScopeKind.Self)
        {
            return CheckInstanceOf(query, context.Selection);
        }

        if (query.ScopeKind == QueryScopeKind.ContainingAncestor)
        {
            return CheckAncestorInstanceOf(query, context);
        }

        // scope=force/roster with category-based filter: check child selections.
        // BattleScribe resolves scope=force/roster to the Force/Roster element itself
        // (not a Selection), so type-based (unit/model/upgrade) and entry-based checks
        // always return false. Only category-based checks can scan child selections.
        if (query.ScopeKind is QueryScopeKind.ContainingForce or QueryScopeKind.ContainingRoster)
        {
            if (query.ValueFilterKind == QueryFilterKind.SpecifiedEntry
                && query.FilterSymbol is ICategoryEntrySymbol)
            {
                bool descSel = query.Options.HasFlag(QueryOptions.IncludeDescendantSelections);
                bool descForce = query.Options.HasFlag(QueryOptions.IncludeDescendantForces);
                var selections = query.ScopeKind == QueryScopeKind.ContainingForce
                    ? ForceSelections(context.Force!, descSel, descForce)
                    : RosterSelections(descSel, descForce);
                return CheckScopeInstanceOf(query, selections);
            }
            return false;
        }

        return false;
    }

    private bool CheckScopeInstanceOf(IQuerySymbol query, IEnumerable<ISelectionSymbol> selections)
    {
        foreach (var sel in selections)
        {
            if (CheckInstanceOf(query, sel))
                return true;
        }
        return false;
    }

    private static bool CheckInstanceOf(IQuerySymbol query, ISelectionSymbol? selection)
    {
        if (selection is null)
            return false;

        // Check by entry type (unit/model/upgrade) — childId was "unit"/"model"/"upgrade"
        if (query.ValueFilterKind is QueryFilterKind.UnitEntry)
            return selection.EntryKind == SelectionEntryKind.Unit;
        if (query.ValueFilterKind is QueryFilterKind.ModelEntry)
            return selection.EntryKind == SelectionEntryKind.Model;
        if (query.ValueFilterKind is QueryFilterKind.UpgradeEntry)
            return selection.EntryKind == SelectionEntryKind.Upgrade;

        // Check by specific entry/category ID
        var filterId = query.FilterSymbol?.Id;
        if (filterId is null)
            return false;

        if (selection.EntryId == filterId)
            return true;
        foreach (var cat in selection.Categories)
        {
            if (cat.SourceEntry?.Id == filterId)
                return true;
        }

        return false;
    }

    private bool CheckAncestorInstanceOf(IQuerySymbol query, EvalContext context)
    {
        if (context.Selection is null)
            return false;
        for (ISymbol? sym = context.Selection.ContainingSymbol; sym is not null; sym = sym.ContainingSymbol)
        {
            if (sym is ISelectionSymbol parentSel && CheckInstanceOf(query, parentSel))
                return true;
            if (sym is IForceSymbol)
                break; // Stop at force boundary
        }
        return false;
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

    // ──────────────────────────────────────────────────────────────────
    //  Layer 1: Tree traversal primitives (pure, static, reusable)
    // ──────────────────────────────────────────────────────────────────

    /// <summary>
    /// Yields a selection and all its recursive descendants (subtree flattened).
    /// </summary>
    private static IEnumerable<ISelectionSymbol> SelectionWithDescendants(ISelectionSymbol selection)
    {
        yield return selection;
        foreach (var child in selection.Selections)
        {
            foreach (var desc in SelectionWithDescendants(child))
                yield return desc;
        }
    }

    /// <summary>
    /// Enumerates selections under a force, optionally including descendant selections
    /// and/or descendant child forces.
    /// </summary>
    private static IEnumerable<ISelectionSymbol> ForceSelections(
        IForceSymbol force, bool descendIntoSelections, bool descendIntoForces)
    {
        foreach (var sel in force.Selections)
        {
            if (descendIntoSelections)
            {
                foreach (var item in SelectionWithDescendants(sel))
                    yield return item;
            }
            else
            {
                yield return sel;
            }
        }
        if (descendIntoForces)
        {
            foreach (var childForce in force.Forces)
            {
                foreach (var item in ForceSelections(childForce, descendIntoSelections, descendIntoForces))
                    yield return item;
            }
        }
    }

    /// <summary>
    /// All selections in the roster across all forces, optionally including
    /// descendant selections and/or descendant child forces.
    /// </summary>
    private IEnumerable<ISelectionSymbol> RosterSelections(bool descendIntoSelections, bool descendIntoForces)
    {
        foreach (var force in _roster.Forces)
        {
            foreach (var item in ForceSelections(force, descendIntoSelections, descendIntoForces))
                yield return item;
        }
    }

    // ──────────────────────────────────────────────────────────────────
    //  Layer 2: Context-aware scope resolution
    // ──────────────────────────────────────────────────────────────────

    /// <summary>
    /// Direct children of the containing element (siblings in parent scope).
    /// When <paramref name="descendIntoSelections"/> is true, also includes each sibling's descendants.
    /// </summary>
    private static IEnumerable<ISelectionSymbol> SiblingSelections(EvalContext context, bool descendIntoSelections)
    {
        if (context.Selection is null)
        {
            if (context.Force is not null)
            {
                foreach (var s in context.Force.Selections)
                {
                    if (descendIntoSelections)
                    {
                        foreach (var item in SelectionWithDescendants(s))
                            yield return item;
                    }
                    else
                    {
                        yield return s;
                    }
                }
            }
            yield break;
        }
        var parent = context.Selection.ContainingSymbol;
        var siblings = parent switch
        {
            ISelectionSymbol parentSel => parentSel.Selections,
            IForceSymbol parentForce => parentForce.Selections,
            _ => Enumerable.Empty<ISelectionSymbol>(),
        };
        foreach (var s in siblings)
        {
            if (descendIntoSelections)
            {
                foreach (var item in SelectionWithDescendants(s))
                    yield return item;
            }
            else
            {
                yield return s;
            }
        }
    }

    /// <summary>
    /// Walk up ancestry to find matching entry, return its subtree.
    /// Falls back to roster selections when no scopeId is specified.
    /// </summary>
    private IEnumerable<ISelectionSymbol> AncestorSelections(
        IQuerySymbol query, EvalContext context, bool descendIntoSelections, bool descendIntoForces)
    {
        if (context.Selection is null || context.Force is null)
            yield break;

        var scopeId = query.ScopeSymbol?.Id;
        if (scopeId is null)
        {
            foreach (var sel in RosterSelections(descendIntoSelections, descendIntoForces))
                yield return sel;
            yield break;
        }

        for (ISymbol? sym = context.Selection; sym is not null; sym = sym.ContainingSymbol)
        {
            if (sym is ISelectionSymbol selSym && MatchesEntryOrGroup(selSym, scopeId))
            {
                foreach (var item in SelectionWithDescendants(selSym))
                    yield return item;
                yield break;
            }
            if (sym is IForceSymbol)
                break;
        }
    }

    // ──────────────────────────────────────────────────────────────────
    //  Layer 3: Scope dispatch + scope filters
    // ──────────────────────────────────────────────────────────────────

    private IEnumerable<ISelectionSymbol> GetSelectionsInScope(IQuerySymbol query, EvalContext context)
    {
        bool descSel = query.Options.HasFlag(QueryOptions.IncludeDescendantSelections);
        bool descForce = query.Options.HasFlag(QueryOptions.IncludeDescendantForces);
        return query.ScopeKind switch
        {
            QueryScopeKind.Self => context.Selection is { } s ? [s] : [],
            QueryScopeKind.Parent => SiblingSelections(context, descSel),
            QueryScopeKind.ContainingForce => context.Force is null ? []
                : ForceSelections(context.Force, descSel, descForce),
            QueryScopeKind.ContainingRoster => RosterSelections(descSel, descForce),
            QueryScopeKind.ContainingAncestor => AncestorSelections(query, context, descSel, descForce),
            QueryScopeKind.ReferencedEntry => FilterByReferencedEntry(query, descSel, descForce),
            QueryScopeKind.PrimaryCategory => FilterByPrimaryCategory(context, descSel, descForce),
            QueryScopeKind.PrimaryCatalogue => context.Force is null ? []
                : ForceSelections(context.Force, descSel, descendIntoForces: false),
            _ => [],
        };
    }

    /// <summary>
    /// Roster selections filtered to those matching a referenced entry/group.
    /// </summary>
    private IEnumerable<ISelectionSymbol> FilterByReferencedEntry(
        IQuerySymbol query, bool descendIntoSelections, bool descendIntoForces)
    {
        var entryId = query.ScopeSymbol?.Id;
        if (entryId is null)
            yield break;

        foreach (var sel in RosterSelections(descendIntoSelections, descendIntoForces))
        {
            if (MatchesEntryOrGroup(sel, entryId))
                yield return sel;
        }
    }

    /// <summary>
    /// Force selections filtered to those sharing the current selection's primary category.
    /// </summary>
    private static IEnumerable<ISelectionSymbol> FilterByPrimaryCategory(
        EvalContext context, bool descendIntoSelections, bool descendIntoForces)
    {
        if (context.Selection is null || context.Force is null)
            yield break;

        var primaryCat = context.Selection.PrimaryCategory?.SourceEntry?.Id;
        if (primaryCat is null)
            yield break;

        foreach (var sel in ForceSelections(context.Force, descendIntoSelections, descendIntoForces))
        {
            if (SelectionHasCategory(sel, primaryCat))
                yield return sel;
        }
    }

    private decimal CountSelections(IEnumerable<ISelectionSymbol> selections, IQuerySymbol query)
    {
        decimal count = 0;
        foreach (var sel in selections)
        {
            if (MatchesFilter(sel, query))
            {
                count += sel.SelectedCount;
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
            count += sel.SelectedCount;
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

    private decimal SumMemberValues(IEnumerable<ISelectionSymbol> selections, IQuerySymbol query)
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
                    if (cost.Type?.Id == valueTypeId)
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

        foreach (var rosterCost in _roster.Costs)
        {
            if (rosterCost.CostType?.Id == valueTypeId && rosterCost.Limit is { } limit)
                return limit;
        }
        return -1m; // No limit
    }

    private static bool MatchesFilter(ISelectionSymbol sel, IQuerySymbol query)
    {
        return query.ValueFilterKind switch
        {
            QueryFilterKind.Anything => true,
            QueryFilterKind.Unknown => true,
            QueryFilterKind.UnitEntry => sel.EntryKind == SelectionEntryKind.Unit,
            QueryFilterKind.ModelEntry => sel.EntryKind == SelectionEntryKind.Model,
            QueryFilterKind.UpgradeEntry => sel.EntryKind == SelectionEntryKind.Upgrade,
            QueryFilterKind.SpecifiedEntry => MatchesSpecifiedEntry(sel, query),
            _ => true,
        };
    }

    private static bool MatchesSpecifiedEntry(ISelectionSymbol sel, IQuerySymbol query)
    {
        var filterSymbol = query.FilterSymbol;
        if (filterSymbol is null or IErrorSymbol)
            return false;
        var filterId = filterSymbol.Id;
        if (filterId is null)
            return false;

        // shared=true: segment-based matching (any segment of "linkId::targetId" can match)
        // shared=false: exact matching only (composite IDs like "link::target" won't match base "target")
        bool isShared = query.Options.HasFlag(QueryOptions.SharedConstraint);

        // Check if selection's entry matches
        if (isShared ? MatchesEntryOrGroup(sel, filterId) : MatchesEntryOrGroupExact(sel, filterId))
            return true;

        // Check if selection has a category matching the filter
        if (SelectionHasCategory(sel, filterId))
            return true;

        return false;
    }

    private static bool MatchesEntryOrGroupExact(ISelectionSymbol sel, string id)
    {
        if (sel.EntryGroupId == id)
            return true;
        return sel.EntryId == id;
    }

    private static bool MatchesEntry(ISelectionSymbol sel, string? entryId)
    {
        if (entryId is null) return false;
        return MatchesEntryOrGroup(sel, entryId);
    }

    private static bool SelectionHasCategory(ISelectionSymbol sel, string? categoryId)
    {
        if (categoryId is null) return false;
        foreach (var cat in sel.Categories)
        {
            if (cat.SourceEntry?.Id == categoryId)
                return true;
        }
        return false;
    }

    private static bool MatchesEntryType(ISelectionSymbol sel, QueryFilterKind filterKind)
    {
        return filterKind switch
        {
            QueryFilterKind.UnitEntry => sel.EntryKind == SelectionEntryKind.Unit,
            QueryFilterKind.ModelEntry => sel.EntryKind == SelectionEntryKind.Model,
            QueryFilterKind.UpgradeEntry => sel.EntryKind == SelectionEntryKind.Upgrade,
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

            // percentValue: scale referenceValue to an absolute threshold
            // e.g. value=25, total=5 → threshold=5*25/100=1.25; count=4; floor(4/1.25)=3
            if (query.Options.HasFlag(QueryOptions.ValuePercentage))
            {
                var total = CountTotalSelectionsInScope(query, context);
                referenceValue = total * referenceValue / 100m;
                if (referenceValue == 0)
                    return 0;
            }

            var roundUp = query.Options.HasFlag(QueryOptions.ValueRoundUp);

            var repeats = value / referenceValue;
            var repeatCount = roundUp ? (int)Math.Ceiling(repeats) : (int)Math.Floor(repeats);
            return Math.Max(0, repeatCount * Math.Max(1, effect.Repetitions));
        }

        // Check repeat children (ModifierEffectBaseSymbol.Effects → RepeatEffectSymbol)
        // Multiple repeats are additive: total = sum of each child's repeat count.
        if (effect.Effects.Length > 0)
        {
            int totalRepeats = 0;
            foreach (var repeatEffect in effect.Effects)
            {
                totalRepeats += GetRepeatCount(repeatEffect, context);
            }
            return totalRepeats;
        }

        return 1;
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
        ISelectionSymbol? Selection,
        IForceSymbol? Force,
        ISymbol EntrySymbol);

    /// <summary>
    /// Checks if a selection matches a given entry or entry group ID.
    /// </summary>
    private static bool MatchesEntryOrGroup(ISelectionSymbol sel, string id)
    {
        if (sel.EntryGroupId == id)
            return true;
        if (sel.EntryId is not { } entryId)
            return false;
        if (entryId == id)
            return true;
        // Entry link format: "linkId::targetId" — check each segment
        var span = entryId.AsSpan();
        while (true)
        {
            var sepIndex = span.IndexOf("::", StringComparison.Ordinal);
            if (sepIndex < 0)
                return span.SequenceEqual(id.AsSpan());
            if (span[..sepIndex].SequenceEqual(id.AsSpan()))
                return true;
            span = span[(sepIndex + 2)..];
        }
    }
}

