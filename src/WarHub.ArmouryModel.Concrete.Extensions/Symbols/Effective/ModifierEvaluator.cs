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

        // Process modifiers on individual category links (e.g. set primary=true on a categoryLink)
        ApplyAllCategoryLinkPrimaryToggles(entry, selection, force, categories, ref primaryId);

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

        // Process modifiers on individual category links
        ApplyAllCategoryLinkPrimaryToggles(entry, selection, force, categories, ref primaryId);

        return (categories, primaryId);
    }

    // ──────────────────────────────────────────────────────────────────
    //  Effect application
    // ──────────────────────────────────────────────────────────────────

    /// <summary>
    /// Iterates all category links on the entry and applies per-link primary toggles.
    /// Unlike <see cref="ApplyEntryCategoryMutation"/>, which operates on the entry's entire
    /// category collection (add/remove/set primary/unset primary), this method processes
    /// each <c>ICategoryLinkSymbol</c> individually, toggling only its primary flag.
    /// </summary>
    private void ApplyAllCategoryLinkPrimaryToggles(
        ISelectionEntryContainerSymbol entry,
        ISelectionSymbol? selection,
        IForceSymbol? force,
        List<string> categories,
        ref string? primaryId)
    {
        foreach (var cat in entry.Categories)
        {
            if (cat.Effects.IsEmpty) continue;
            var catId = cat.ReferencedEntry?.Id ?? cat.Id;
            if (catId is null) continue;
            var catContext = new EvalContext(selection, force, cat);
            foreach (var effect in cat.Effects)
            {
                ApplyCategoryLinkPrimaryToggle(effect, catId, categories, ref primaryId, catContext);
            }
        }
    }

    /// <summary>
    /// Toggles the primary flag on a specific category link.
    /// This operates at the link level — it only sets or unsets the primary flag for the
    /// category identified by <paramref name="catId"/>. Compare with
    /// <see cref="ApplyEntryCategoryMutation"/> which operates on the entry's entire category
    /// collection (add/remove categories, set/unset primary).
    /// </summary>
    private void ApplyCategoryLinkPrimaryToggle(
        IEffectSymbol effect, string catId,
        List<string> categories, ref string? primaryId, EvalContext context)
    {
        if (effect.TargetKind == EffectTargetKind.CategoryPrimary &&
            effect.FunctionKind == EffectOperation.SetValue &&
            EvaluateEffectCondition(effect, context))
        {
            if (ParseBool(effect.OperandValue))
            {
                primaryId = catId;
                if (!categories.Contains(catId))
                    categories.Add(catId);
            }
            else if (primaryId == catId)
            {
                primaryId = null;
            }
        }

        // Process children (modifier groups)
        if (effect.ChildrenWhenSatisfied.Length > 0 || effect.ChildrenWhenUnsatisfied.Length > 0)
        {
            var satisfied = EvaluateCondition(effect.Condition, context);
            var children = satisfied ? effect.ChildrenWhenSatisfied : effect.ChildrenWhenUnsatisfied;
            foreach (var child in children)
            {
                ApplyCategoryLinkPrimaryToggle(child, catId, categories, ref primaryId, context);
            }
        }
    }

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
    /// This operates at the entry level on the entire category collection. Compare with
    /// <see cref="ApplyCategoryLinkPrimaryToggle"/> which only toggles the primary flag
    /// on a specific category link.
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
                var selections = query.ScopeKind == QueryScopeKind.ContainingForce
                    ? GetForceSelections(context)
                    : GetRosterSelections();
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

    private IEnumerable<ISelectionSymbol> GetSelectionsInScope(IQuerySymbol query, EvalContext context)
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

    private IEnumerable<ISelectionSymbol> GetSelfSelections(EvalContext context)
    {
        if (context.Selection is { } sel)
            yield return sel;
    }

    private IEnumerable<ISelectionSymbol> GetParentSelections(EvalContext context)
    {
        if (context.Selection is null)
        {
            if (context.Force is not null)
            {
                foreach (var s in context.Force.Selections)
                    yield return s;
            }
            yield break;
        }
        // Parent is just ContainingSymbol
        var parent = context.Selection.ContainingSymbol;
        if (parent is ISelectionSymbol parentSel)
        {
            // Return siblings (parent's children)
            foreach (var s in parentSel.Selections)
                yield return s;
        }
        else if (parent is IForceSymbol parentForce)
        {
            // Root selection — parent is force, return force-level selections
            foreach (var s in parentForce.Selections)
                yield return s;
        }
    }

    private IEnumerable<ISelectionSymbol> GetForceSelections(EvalContext context)
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

    private IEnumerable<ISelectionSymbol> GetRosterSelections()
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

    private IEnumerable<ISelectionSymbol> GetAncestorSelections(IQuerySymbol query, EvalContext context)
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

        // Walk up ContainingSymbol chain looking for matching ancestor
        for (ISymbol? sym = context.Selection; sym is not null; sym = sym.ContainingSymbol)
        {
            if (sym is ISelectionSymbol selSym && MatchesEntryOrGroup(selSym, scopeId))
            {
                yield return selSym;
                foreach (var desc in GetDescendantSelections(selSym))
                    yield return desc;
                yield break;
            }
            if (sym is IForceSymbol)
                break;
        }
    }

    private IEnumerable<ISelectionSymbol> GetReferencedEntrySelections(IQuerySymbol query)
    {
        // Scope is a specific entry — find all selections matching that entry across the roster
        var entryId = query.ScopeSymbol?.Id;
        if (entryId is null)
            yield break;

        foreach (var sel in GetRosterSelections())
        {
            if (MatchesEntryOrGroup(sel, entryId))
                yield return sel;
        }
    }

    private IEnumerable<ISelectionSymbol> GetPrimaryCategorySelections(EvalContext context)
    {
        // Find the primary category of the current selection's root,
        // then find all selections in the force with that category
        if (context.Selection is null || context.Force is null)
            yield break;

        var primaryCat = context.Selection.PrimaryCategory?.SourceEntry?.Id;

        if (primaryCat is null)
            yield break;

        foreach (var sel in GetForceSelections(context))
        {
            if (SelectionHasCategory(sel, primaryCat))
                yield return sel;
        }
    }

    private static IEnumerable<ISelectionSymbol> GetDescendantSelections(ISelectionSymbol sel)
    {
        foreach (var child in sel.Selections)
        {
            yield return child;
            foreach (var desc in GetDescendantSelections(child))
                yield return desc;
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
        var filterId = filterSymbol?.Id;
        if (filterId is null)
            return false;

        // Check if selection's entry matches
        if (MatchesEntryOrGroup(sel, filterId))
            return true;

        // Check if selection has a category matching the filter
        if (SelectionHasCategory(sel, filterId))
            return true;

        return false;
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
        return sel.EntryId == id || sel.EntryGroupId == id;
    }
}

