using WarHub.ArmouryModel.Concrete;
using WarHub.ArmouryModel.EditorServices;
using WarHub.ArmouryModel.Source;

namespace WarHub.ArmouryModel.RosterEngine;

/// <summary>
/// ISymbol-based roster engine. Operates on <see cref="WhamCompilation"/> with a functional API.
/// Each operation accepts the current <see cref="RosterState"/> (or compilation) and returns a
/// new <see cref="RosterState"/> containing the updated, immutable roster tree.
/// This enables straightforward undo/redo via state snapshots.
/// </summary>
public sealed class WhamRosterEngine
{
    /// <summary>
    /// Separator used in entry link IDs: "linkId::targetId".
    /// </summary>
    internal const string EntryLinkIdSeparator = "::";

    // ──────────────────────────────────────────────────────────────────────
    //  Roster lifecycle
    // ──────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Creates a new, empty roster for the given catalogue compilation.
    /// The compilation must contain exactly one gamesystem (root catalogue).
    /// Cost types defined in the gamesystem are used to initialise
    /// the roster's <see cref="RosterNode.Costs"/> and <see cref="RosterNode.CostLimits"/>.
    /// </summary>
    /// <param name="catalogCompilation">
    /// A <see cref="WhamCompilation"/> that contains at least a gamesystem source tree.
    /// </param>
    /// <param name="name">Optional roster name. When <see langword="null"/>, a default name is generated.</param>
    /// <returns>A new <see cref="RosterState"/> with the roster added to the compilation.</returns>
    /// <exception cref="InvalidOperationException">The compilation has no gamesystem declaration.</exception>
    public RosterState CreateRoster(WhamCompilation catalogCompilation, string? name = null)
    {
        var gsSym = catalogCompilation.GlobalNamespace.RootCatalogue;
        var gsDecl = gsSym.GetGamesystemDeclaration()
            ?? throw new InvalidOperationException("Compilation contains no gamesystem declaration.");

        // Build roster with cost limits and zero-value costs seeded from the gamesystem's cost types,
        // mirroring the pattern established by CreateRosterOperation in EditorServices.
        var rosterNode = NodeFactory.Roster(gsDecl, name)
            .WithCostLimits(gsDecl.CostTypes
                .Select(ct => NodeFactory.CostLimit(ct, ct.DefaultCostLimit))
                .ToArray())
            .WithCosts(gsDecl.CostTypes
                .Select(ct => NodeFactory.Cost(ct, 0m))
                .ToArray());

        var rosterTree = SourceTree.CreateForRoot(rosterNode);
        var compilation = WhamCompilation.CreateRosterCompilation([rosterTree], catalogCompilation);
        return new RosterState(compilation);
    }

    // ──────────────────────────────────────────────────────────────────────
    //  Force lifecycle
    // ──────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Adds a force to the roster using the given force entry definition.
    /// Categories declared on the force entry (via category links) are resolved
    /// through the symbol's <see cref="ICategoryEntrySymbol.ReferencedEntry"/> and
    /// added to the new <see cref="ForceNode"/>.
    /// </summary>
    /// <param name="state">Current roster state.</param>
    /// <param name="forceEntry">The force entry symbol to instantiate.</param>
    /// <param name="catalogue">
    /// The catalogue that owns <paramref name="forceEntry"/>. Reserved for future use
    /// (e.g. catalogue-scoped cost type injection).
    /// </param>
    /// <returns>A new <see cref="RosterState"/> with the force appended.</returns>
    /// <exception cref="ArgumentException"><paramref name="forceEntry"/> has no backing declaration node.</exception>
    public RosterState AddForce(
        RosterState state,
        IForceEntrySymbol forceEntry,
        ICatalogueSymbol catalogue)
    {
        var roster = state.RosterRequired;
        var forceEntryDecl = forceEntry.GetDeclaration()
            ?? throw new ArgumentException("Force entry symbol has no backing declaration node.", nameof(forceEntry));

        // Resolve the catalogue's backing declaration node so the force records
        // the correct catalogueId (not the force entry's defining catalogue).
        var catalogueDecl = catalogue.GetDeclaration() as CatalogueBaseNode;

        var forceNode = NodeFactory.Force(forceEntryDecl, catalogueDecl);

        // Resolve category links declared on the force entry into CategoryNodes.
        var categories = BuildForceCategories(forceEntry);
        if (categories.Length > 0)
        {
            forceNode = forceNode.AddCategories(categories);
        }

        var newRoster = roster.AddForces(forceNode).WithUpdatedCostTotals();
        return state.ReplaceRoster(newRoster);
    }

    /// <summary>
    /// Removes the force at the specified index from the roster.
    /// </summary>
    /// <param name="state">Current roster state.</param>
    /// <param name="forceIndex">Zero-based index of the force to remove.</param>
    /// <returns>A new <see cref="RosterState"/> without the specified force.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="forceIndex"/> is negative or greater than or equal to the number of forces.
    /// </exception>
    public RosterState RemoveForce(RosterState state, int forceIndex)
    {
        var roster = state.RosterRequired;
        ValidateForceIndex(roster, forceIndex);

        var forceToRemove = roster.Forces[forceIndex];
        var newRoster = roster.Remove(forceToRemove).WithUpdatedCostTotals();
        return state.ReplaceRoster(newRoster);
    }

    // ──────────────────────────────────────────────────────────────────────
    //  Entry resolution
    // ──────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Gets the available entries for a force, using the catalogue symbol associated with it.
    /// The catalogue is resolved from the force's <see cref="ForceNode.CatalogueId"/>.
    /// </summary>
    /// <param name="state">Current roster state.</param>
    /// <param name="forceIndex">Zero-based index of the force.</param>
    /// <returns>Ordered list of available entries for selection in the force's catalogue.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="forceIndex"/> is out of range.</exception>
    /// <exception cref="InvalidOperationException">No catalogue found for the force's catalogue ID.</exception>
    public IReadOnlyList<AvailableEntry> GetAvailableEntries(RosterState state, int forceIndex)
    {
        var roster = state.RosterRequired;
        ValidateForceIndex(roster, forceIndex);

        var force = roster.Forces[forceIndex];
        var catalogue = ResolveForceCatalogue(state, force);
        return EntryResolver.GetAvailableEntries(catalogue);
    }

    /// <summary>
    /// Gets the available child entries for a specific entry symbol.
    /// Delegates to <see cref="EntryResolver.GetChildEntries"/>.
    /// </summary>
    public IReadOnlyList<AvailableEntry> GetChildEntries(ISelectionEntryContainerSymbol entry)
    {
        return EntryResolver.GetChildEntries(entry);
    }

    // ──────────────────────────────────────────────────────────────────────
    //  Selection lifecycle
    // ──────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Selects an entry and adds it to the specified force.
    /// Creates a <see cref="SelectionNode"/> from the entry symbol's declaration,
    /// assigns categories and costs, and auto-selects child entries that have
    /// minimum constraints requiring at least one instance.
    /// </summary>
    /// <param name="state">Current roster state.</param>
    /// <param name="forceIndex">Zero-based index of the target force.</param>
    /// <param name="entry">The entry symbol to select.</param>
    /// <param name="sourceGroup">
    /// The group this entry was flattened from, if any.
    /// Used to populate <see cref="SelectionNode.EntryGroupId"/>.
    /// </param>
    /// <returns>A new <see cref="RosterState"/> with the selection added.</returns>
    public RosterState SelectEntry(
        RosterState state,
        int forceIndex,
        ISelectionEntryContainerSymbol entry,
        ISelectionEntryGroupSymbol? sourceGroup = null)
    {
        var roster = state.RosterRequired;
        ValidateForceIndex(roster, forceIndex);

        var selectionNode = CreateSelectionWithAutoChildren(entry, sourceGroup);
        var force = roster.Forces[forceIndex];
        var newForce = force.AddSelections(selectionNode);
        var newRoster = roster.Replace(force, _ => newForce).WithUpdatedCostTotals();
        return state.ReplaceRoster(newRoster);
    }

    /// <summary>
    /// Selects a child entry and nests it under an existing selection.
    /// </summary>
    /// <param name="state">Current roster state.</param>
    /// <param name="forceIndex">Zero-based index of the force containing the parent selection.</param>
    /// <param name="selectionIndex">Zero-based index of the parent selection within the force.</param>
    /// <param name="childEntry">The child entry symbol to select.</param>
    /// <param name="sourceGroup">The group this entry was flattened from, if any.</param>
    /// <returns>A new <see cref="RosterState"/> with the child selection appended.</returns>
    public RosterState SelectChildEntry(
        RosterState state,
        int forceIndex,
        int selectionIndex,
        ISelectionEntryContainerSymbol childEntry,
        ISelectionEntryGroupSymbol? sourceGroup = null)
    {
        var roster = state.RosterRequired;
        ValidateForceIndex(roster, forceIndex);

        var force = roster.Forces[forceIndex];
        ValidateSelectionIndex(force, selectionIndex);

        var parentSelection = force.Selections[selectionIndex];
        var childSelectionNode = CreateSelectionWithAutoChildren(childEntry, sourceGroup);
        var newParent = parentSelection.AddSelections(childSelectionNode);
        var newRoster = roster.Replace(parentSelection, _ => newParent).WithUpdatedCostTotals();
        return state.ReplaceRoster(newRoster);
    }

    /// <summary>
    /// Removes a selection from a force.
    /// </summary>
    /// <param name="state">Current roster state.</param>
    /// <param name="forceIndex">Zero-based index of the force.</param>
    /// <param name="selectionIndex">Zero-based index of the selection to remove.</param>
    /// <returns>A new <see cref="RosterState"/> without the specified selection.</returns>
    public RosterState DeselectSelection(RosterState state, int forceIndex, int selectionIndex)
    {
        var roster = state.RosterRequired;
        ValidateForceIndex(roster, forceIndex);

        var force = roster.Forces[forceIndex];
        ValidateSelectionIndex(force, selectionIndex);

        var selectionToRemove = force.Selections[selectionIndex];
        var newRoster = roster.Remove(selectionToRemove).WithUpdatedCostTotals();
        return state.ReplaceRoster(newRoster);
    }

    /// <summary>
    /// Duplicates a selection within a force by appending a structural copy.
    /// The duplicate carries the same entry data but is a distinct roster element.
    /// </summary>
    /// <param name="state">Current roster state.</param>
    /// <param name="forceIndex">Zero-based index of the force.</param>
    /// <param name="selectionIndex">Zero-based index of the selection to duplicate.</param>
    /// <returns>A new <see cref="RosterState"/> with the duplicate appended to the force.</returns>
    public RosterState DuplicateSelection(RosterState state, int forceIndex, int selectionIndex)
    {
        var roster = state.RosterRequired;
        ValidateForceIndex(roster, forceIndex);

        var force = roster.Forces[forceIndex];
        ValidateSelectionIndex(force, selectionIndex);

        var original = force.Selections[selectionIndex];
        var duplicate = RegenerateIds(original);
        var newForce = force.AddSelections(duplicate);
        var newRoster = roster.Replace(force, _ => newForce).WithUpdatedCostTotals();
        return state.ReplaceRoster(newRoster);
    }

    /// <summary>
    /// Sets the cost limit for a specific cost type on the roster.
    /// The cost type must already exist in the roster's <see cref="RosterNode.CostLimits"/>
    /// (seeded from the gamesystem during <see cref="CreateRoster"/>).
    /// </summary>
    /// <param name="state">Current roster state.</param>
    /// <param name="costTypeId">The <see cref="CostLimitNode.TypeId"/> to update.</param>
    /// <param name="value">The new limit value.</param>
    /// <returns>A new <see cref="RosterState"/> with the updated cost limit.</returns>
    /// <exception cref="ArgumentException">No cost limit with the given type ID exists.</exception>
    public RosterState SetCostLimit(RosterState state, string costTypeId, decimal value)
    {
        var roster = state.RosterRequired;
        var targets = roster.CostLimits.Where(cl => cl.TypeId == costTypeId).ToArray();
        if (targets.Length == 0)
        {
            throw new ArgumentException(
                $"No cost limit with type ID '{costTypeId}' found on the roster.",
                nameof(costTypeId));
        }

        var newRoster = roster.Replace<RosterNode, CostLimitNode>(targets, cl => cl.WithValue(value));
        return state.ReplaceRoster(newRoster);
    }

    // TODO Phase 4: EvaluateModifiers — apply IEffectSymbol / IConditionSymbol modifiers
    // TODO Phase 5: ValidateConstraints — evaluate IConstraintSymbol / IQuerySymbol rules

    // ──────────────────────────────────────────────────────────────────────
    //  Selection creation helpers
    // ──────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Creates a <see cref="SelectionNode"/> from an entry symbol, including costs,
    /// categories, and recursively auto-selected child entries (those with min ≥ 1
    /// constraints on <c>selections</c> in <c>parent</c> scope).
    /// This public overload allows the adapter layer to create selections for nested forces
    /// where the index-based engine API cannot be used directly.
    /// </summary>
    public SelectionNode CreateSelectionFromEntry(
        ISelectionEntryContainerSymbol entry,
        ISelectionEntryGroupSymbol? sourceGroup)
        => CreateSelectionWithAutoChildren(entry, sourceGroup);

    /// <summary>
    /// Creates a <see cref="SelectionNode"/> from an entry symbol, including costs,
    /// categories, and recursively auto-selected child entries (those with min ≥ 1
    /// constraints on <c>selections</c> in <c>parent</c> scope).
    /// </summary>
    /// <param name="entry">The entry symbol to create a selection for.</param>
    /// <param name="sourceGroup">The group this entry was flattened from, if any.</param>
    /// <param name="linkPrefix">
    /// The accumulated link prefix from parent links above this entry.
    /// Propagated to child entries so that their <c>entryId</c> values are correctly
    /// prefixed with all link IDs in the hierarchy above them.
    /// </param>
    private SelectionNode CreateSelectionWithAutoChildren(
        ISelectionEntryContainerSymbol entry,
        ISelectionEntryGroupSymbol? sourceGroup,
        string linkPrefix = "")
    {
        var selectionNode = CreateSelectionNode(entry, sourceGroup, linkPrefix);

        // Auto-select child entries that have a minimum constraint ≥ 1.
        // Pass linkPrefix so GetChildEntries computes each child's LinkPrefix correctly.
        var childEntries = EntryResolver.GetChildEntries(entry, linkPrefix);

        // Track which groups we've already auto-selected for (to avoid duplicates)
        var autoSelectedGroups = new HashSet<string>(StringComparer.Ordinal);

        foreach (var child in childEntries)
        {
            // Check 1: Does the individual entry have a min constraint?
            var autoCount = GetMinSelectionCount(child.Symbol);

            // Check 2: Does the source group have a min constraint?
            if (autoCount < 1 && child.SourceGroup is { Id: { } groupId } group)
            {
                if (!autoSelectedGroups.Contains(groupId))
                {
                    var groupMin = GetGroupMinSelectionCount(group);
                    if (groupMin >= 1 && IsDefaultEntryForGroup(child.Symbol, group))
                    {
                        autoCount = groupMin;
                        autoSelectedGroups.Add(groupId);
                    }
                }
            }

            if (autoCount < 1)
            {
                continue;
            }

            // Create one child selection with Number set to the min count.
            // Use child.LinkPrefix so the child's entryId is correctly prefixed.
            var childSelection = CreateSelectionWithAutoChildren(child.Symbol, child.SourceGroup, child.LinkPrefix);
            if (autoCount > 1)
            {
                childSelection = childSelection.WithNumber(autoCount);
            }
            selectionNode = selectionNode.AddSelections(childSelection);
        }

        return selectionNode;
    }

    /// <summary>
    /// Checks if a group has a minimum selection count constraint.
    /// Uses the symbol API (via <see cref="GetEffectiveConstraints"/>).
    /// </summary>
    private static int GetGroupMinSelectionCount(ISelectionEntryGroupSymbol group)
    {
        foreach (var constraint in GetEffectiveConstraints(group))
        {
            if (IsMinSelectionConstraint(constraint))
            {
                return (int)(constraint.Query.ReferenceValue ?? 0);
            }
        }
        return 0;
    }

    /// <summary>
    /// Checks if the given entry is the default entry for its group.
    /// </summary>
    private static bool IsDefaultEntryForGroup(
        ISelectionEntryContainerSymbol entry,
        ISelectionEntryGroupSymbol group)
    {
        var defaultEntry = group.DefaultSelectionEntry;
        if (defaultEntry is not null)
        {
            return MatchesDefaultEntry(entry, defaultEntry);
        }

        // If no explicit default, check if this is the only entry in the group
        var count = 0;
        foreach (var _ in group.ChildSelectionEntries)
        {
            count++;
            if (count > 1) return false;
        }
        return count == 1;
    }

    /// <summary>
    /// Checks whether <paramref name="entry"/> matches <paramref name="defaultEntry"/>
    /// by comparing IDs and resolved target IDs.
    /// </summary>
    private static bool MatchesDefaultEntry(
        ISelectionEntryContainerSymbol entry,
        ISelectionEntryContainerSymbol defaultEntry)
    {
        var entryResolved = EntryResolver.ResolveEntry(entry);
        var defaultResolved = EntryResolver.ResolveEntry(defaultEntry);
        return entryResolved.Id == defaultResolved.Id
            || entry.Id == defaultEntry.Id
            || entry.ReferencedEntry?.Id == defaultEntry.Id
            || entry.ReferencedEntry?.Id == defaultResolved.Id;
    }

    /// <summary>
    /// Creates a <see cref="SelectionNode"/> from an <see cref="ISelectionEntryContainerSymbol"/>.
    /// Resolves the backing <see cref="SelectionEntryNode"/> declaration through links,
    /// then populates costs and categories.
    /// </summary>
    /// <param name="entry">The entry symbol to create a selection for.</param>
    /// <param name="sourceGroup">The group this entry was flattened from, if any.</param>
    /// <param name="linkPrefix">
    /// The accumulated link prefix from parent links above this entry.
    /// Prepended to the entry's own ID path to form the composite <c>entryId</c>.
    /// </param>
    private static SelectionNode CreateSelectionNode(
        ISelectionEntryContainerSymbol entry,
        ISelectionEntryGroupSymbol? sourceGroup,
        string linkPrefix = "")
    {
        var entryDecl = ResolveToEntryDeclaration(entry)
            ?? throw new ArgumentException(
                $"Cannot resolve entry '{entry.Name}' (ID: {entry.Id}) to a SelectionEntryNode declaration.",
                nameof(entry));

        // Build entryId path matching BattleScribe format:
        // For entry links, use "linkId::targetId" so the binder resolves
        // SourceEntryPath = [link, target] with SourceEntry = target.
        // For direct entries, use just the entry's ID.
        // When a linkPrefix is present (from parent links), it is prepended.
        var entryId = BuildEntryIdPath(entry, linkPrefix);
        var entryGroupId = sourceGroup?.Id is { Length: > 0 }
            ? EntryResolver.JoinLinkPrefix(linkPrefix, sourceGroup.Id)
            : null;

        var selectionNode = NodeFactory.Selection(entryDecl, entryId, entryGroupId);

        // Costs: use the entry's own costs, falling back to the resolved target's costs for links.
        var costs = BuildSelectionCosts(entry);
        if (costs.Length > 0)
        {
            selectionNode = selectionNode.AddCosts(costs);
        }

        // Categories: use the entry's own category links, falling back to the target's.
        var categories = BuildSelectionCategories(entry, sourceGroup);
        if (categories.Length > 0)
        {
            selectionNode = selectionNode.AddCategories(categories);
        }

        return selectionNode;
    }

    /// <summary>
    /// Resolves an <see cref="ISelectionEntryContainerSymbol"/> through any link chain
    /// to find the underlying <see cref="SelectionEntryNode"/> declaration.
    /// Falls back to synthesising a declaration from a group if the resolved symbol is a group.
    /// </summary>
    private static SelectionEntryNode? ResolveToEntryDeclaration(ISelectionEntryContainerSymbol symbol)
    {
        var current = symbol;
        for (var depth = 0; depth < 32; depth++)
        {
            var decl = current.GetEntryDeclaration();
            if (decl is not null)
            {
                return decl;
            }

            if (current.ReferencedEntry is not { } next)
            {
                break;
            }
            current = next;
        }

        // If the resolved symbol is a group, synthesise a SelectionEntryNode from it.
        var resolved = EntryResolver.ResolveEntry(symbol);
        var groupDecl = resolved.GetEntryGroupDeclaration();
        if (groupDecl is not null)
        {
            return NodeFactory.SelectionEntry(groupDecl.Name, groupDecl.Id);
        }

        return null;
    }

    /// <summary>
    /// Builds the <c>entryId</c> for a selection node, matching BattleScribe's
    /// <c>"::"</c>-separated path format. For entry links, the path is
    /// <c>"linkId::targetId"</c> so that the binder produces
    /// <c>SourceEntryPath = [link, resolvedTarget]</c>. For direct entries,
    /// returns just the entry's own ID. When a <paramref name="linkPrefix"/> is provided
    /// (from parent links higher in the hierarchy), it is prepended to the built path.
    /// </summary>
    private static string BuildEntryIdPath(
        ISelectionEntryContainerSymbol entry,
        string linkPrefix = "")
    {
        if (entry.ReferencedEntry is { Id: { } targetId })
        {
            // For links: prefix::linkId::targetId (skipping empty segments)
            return EntryResolver.JoinLinkPrefix(
                EntryResolver.JoinLinkPrefix(linkPrefix, entry.Id ?? ""),
                targetId);
        }
        return EntryResolver.JoinLinkPrefix(linkPrefix, entry.Id ?? "");
    }

    /// <summary>
    /// Builds <see cref="CostNode"/> array for a new selection.
    /// Uses the entry's own costs; for links with no costs, falls back to the resolved target.
    /// </summary>
    private static CostNode[] BuildSelectionCosts(ISelectionEntryContainerSymbol entry)
    {
        var costs = entry.Costs;

        // Links may not carry their own costs — inherit from the resolved target.
        if (costs.IsEmpty && entry.ReferencedEntry is { } referenced)
        {
            costs = referenced.Costs;
        }

        if (costs.IsEmpty)
        {
            return [];
        }

        var list = new List<CostNode>(costs.Length);
        foreach (var costSym in costs)
        {
            var costDecl = costSym.GetDeclaration();
            if (costDecl is not null)
            {
                list.Add(costDecl);
            }
        }
        return [.. list];
    }

    /// <summary>
    /// Builds <see cref="CategoryNode"/> array for a new selection.
    /// Resolves category links through <see cref="ICategoryEntrySymbol.ReferencedEntry"/>
    /// and preserves the <see cref="ICategoryEntrySymbol.IsPrimaryCategory"/> flag.
    /// If <paramref name="sourceGroup"/> is provided, its categories are also inherited
    /// (without duplicating entries already present from the entry itself).
    /// </summary>
    private static CategoryNode[] BuildSelectionCategories(
        ISelectionEntryContainerSymbol entry,
        ISelectionEntryGroupSymbol? sourceGroup)
    {
        var categories = entry.Categories;

        // Links may not carry their own categories — inherit from the resolved target.
        if (categories.IsEmpty && entry.ReferencedEntry is { } referenced)
        {
            categories = referenced.Categories;
        }

        var list = new List<CategoryNode>();
        var addedIds = new HashSet<string>(StringComparer.Ordinal);

        foreach (var catSym in categories)
        {
            var targetEntry = catSym.ReferencedEntry ?? catSym;
            var catEntryDecl = targetEntry.GetEntryDeclaration();
            if (catEntryDecl is null)
            {
                continue;
            }

            list.Add(
                NodeFactory.Category(catEntryDecl)
                    .WithPrimary(catSym.IsPrimaryCategory));
            addedIds.Add(catEntryDecl.Id ?? "");
        }

        // Inherit categories from the source group (deduplicated).
        if (sourceGroup is not null)
        {
            foreach (var catSym in sourceGroup.Categories)
            {
                var targetEntry = catSym.ReferencedEntry ?? catSym;
                var catEntryDecl = targetEntry.GetEntryDeclaration();
                if (catEntryDecl is null || !addedIds.Add(catEntryDecl.Id ?? ""))
                {
                    continue;
                }

                list.Add(
                    NodeFactory.Category(catEntryDecl)
                        .WithPrimary(catSym.IsPrimaryCategory));
            }
        }

        return [.. list];
    }

    // ──────────────────────────────────────────────────────────────────────
    //  Constraint inspection helpers
    // ──────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Inspects the constraints on an entry (and its resolved target, for links)
    /// to determine the minimum selection count required by a <c>min</c> constraint
    /// on <c>selections</c> in <c>parent</c> or <c>force</c> scope.
    /// Returns 0 if no such constraint exists.
    /// </summary>
    private static int GetMinSelectionCount(ISelectionEntryContainerSymbol entry)
    {
        foreach (var constraint in GetEffectiveConstraints(entry))
        {
            if (IsMinSelectionConstraint(constraint))
            {
                return (int)(constraint.Query.ReferenceValue ?? 0);
            }
        }
        return 0;
    }

    /// <summary>
    /// Returns <see langword="true"/> if the constraint represents a minimum-selections
    /// requirement in parent or force scope (the auto-select trigger).
    /// Equivalent to the legacy check: <c>type=min, field=selections, scope=parent|force, value≥1</c>.
    /// </summary>
    private static bool IsMinSelectionConstraint(IConstraintSymbol constraint)
    {
        var q = constraint.Query;

        return q.Comparison == QueryComparisonType.GreaterThanOrEqual
            && q.ValueKind == QueryValueKind.SelectionCount
            && q.ScopeKind is QueryScopeKind.Parent or QueryScopeKind.ContainingForce
            && q.ReferenceValue >= 1
            && !q.Options.HasFlag(QueryOptions.ValuePercentage);
    }

    /// <summary>
    /// Yields constraints from the entry itself and, for links, from the resolved target.
    /// This ensures that constraints defined on either the link or the target are considered
    /// during auto-selection evaluation.
    /// </summary>
    private static IEnumerable<IConstraintSymbol> GetEffectiveConstraints(
        ISelectionEntryContainerSymbol entry)
    {
        foreach (var c in entry.Constraints)
        {
            yield return c;
        }

        if (entry.ReferencedEntry is { } referenced)
        {
            foreach (var c in referenced.Constraints)
            {
                yield return c;
            }
        }
    }

    // ──────────────────────────────────────────────────────────────────────
    //  Collective entry helpers
    // ──────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Checks whether the resolved source entry for a symbol is collective.
    /// Resolves through links to check the target entry.
    /// </summary>
    private static bool IsEntryCollective(ISelectionEntryContainerSymbol entry)
    {
        var resolved = EntryResolver.ResolveEntry(entry);
        return resolved.IsCollective;
    }

    /// <summary>
    /// Determines if an entry uses "number-increment" mode for repeated selections.
    /// An entry is number-increment when ALL of its resolved children are either
    /// collective or hidden. If any child is non-collective AND visible, the entry
    /// uses "separate-node" mode instead.
    /// </summary>
    private static bool IsNumberIncrementEntry(ISelectionEntryContainerSymbol entry)
    {
        var resolved = EntryResolver.ResolveEntry(entry);
        return AreAllChildrenCollectiveOrHidden(resolved);

        static bool AreAllChildrenCollectiveOrHidden(ISelectionEntryContainerSymbol e)
        {
            foreach (var child in e.ChildSelectionEntries)
            {
                var childResolved = EntryResolver.ResolveEntry(child);
                if (!childResolved.IsCollective && !childResolved.IsHidden)
                    return false;
                // Recursively check groups (their flattened children)
                if (childResolved.ContainerKind == ContainerKind.SelectionGroup
                    && !AreAllChildrenCollectiveOrHidden(childResolved))
                    return false;
            }
            return true;
        }
    }

    /// <summary>
    /// Recursively scales all child selections proportionally when a parent's number changes.
    /// Each child's new number = child.Number × newParentNumber / oldParentNumber.
    /// </summary>
    private static SelectionNode ScaleChildSelections(SelectionNode sel, int oldNumber, int newNumber)
    {
        if (oldNumber == newNumber || oldNumber == 0 || sel.Selections.Count == 0)
            return sel;

        var newChildren = new List<SelectionNode>(sel.Selections.Count);
        foreach (var child in sel.Selections)
        {
            var scaledChildNumber = child.Number * newNumber / oldNumber;
            if (scaledChildNumber < 1) scaledChildNumber = 1;
            var scaledChild = child.WithUpdatedNumberAndCosts(scaledChildNumber);
            // Recurse into grandchildren
            scaledChild = ScaleChildSelections(scaledChild, child.Number, scaledChildNumber);
            newChildren.Add(scaledChild);
        }

        return sel.WithSelections(sel.Selections.WithNodes(NodeList.Create(newChildren)));
    }

    // ──────────────────────────────────────────────────────────────────────
    //  Force category helpers
    // ──────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Resolves the category links on a force entry into <see cref="CategoryNode"/> instances.
    /// Each <see cref="ICategoryEntrySymbol"/> in <see cref="IForceEntrySymbol.Categories"/> is
    /// typically a category link whose <see cref="ICategoryEntrySymbol.ReferencedEntry"/> points
    /// to the actual category entry definition. The <see cref="ICategoryEntrySymbol.IsPrimaryCategory"/>
    /// flag is preserved on the resulting node.
    /// </summary>
    private static CategoryNode[] BuildForceCategories(IForceEntrySymbol forceEntry)
    {
        var list = new List<CategoryNode>();
        foreach (var catSym in forceEntry.Categories)
        {
            // Category links resolve through ReferencedEntry; direct entries resolve via GetEntryDeclaration.
            var targetEntry = catSym.ReferencedEntry ?? catSym;
            var catEntryDecl = targetEntry.GetEntryDeclaration();
            if (catEntryDecl is null)
            {
                continue;
            }

            list.Add(
                NodeFactory.Category(catEntryDecl)
                    .WithPrimary(catSym.IsPrimaryCategory));
        }
        return [.. list];
    }

    // ──────────────────────────────────────────────────────────────────────
    //  Catalogue resolution & validation
    // ──────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Resolves the <see cref="ICatalogueSymbol"/> for a force using the force's
    /// <see cref="ForceNode.CatalogueId"/>. Searches all catalogues in the compilation
    /// including the gamesystem (since a force may originate from either).
    /// </summary>
    private static ICatalogueSymbol ResolveForceCatalogue(RosterState state, ForceNode force)
    {
        var catalogueId = force.CatalogueId;
        return state.Compilation.GlobalNamespace.Catalogues
            .FirstOrDefault(c => c.Id == catalogueId)
            ?? throw new InvalidOperationException(
                $"No catalogue with ID '{catalogueId}' found in the compilation.");
    }

    private static void ValidateForceIndex(RosterNode roster, int forceIndex)
    {
        if ((uint)forceIndex >= (uint)roster.Forces.Count)
        {
            throw new ArgumentOutOfRangeException(
                nameof(forceIndex),
                forceIndex,
                $"Force index must be in [0, {roster.Forces.Count}). Roster contains {roster.Forces.Count} force(s).");
        }
    }

    private static void ValidateSelectionIndex(ForceNode force, int selectionIndex)
    {
        if ((uint)selectionIndex >= (uint)force.Selections.Count)
        {
            throw new ArgumentOutOfRangeException(
                nameof(selectionIndex),
                selectionIndex,
                $"Selection index must be in [0, {force.Selections.Count}). Force contains {force.Selections.Count} selection(s).");
        }
    }

    // ──────────────────────────────────────────────────────────────────────
    //  ID-based tree traversal
    // ──────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Finds a force node by its instance ID anywhere in the roster's force tree (including nested child forces).
    /// </summary>
    internal static ForceNode? FindForceDeep(RosterNode roster, string forceId)
    {
        foreach (var force in roster.Forces)
        {
            var found = FindForceDeep(force, forceId);
            if (found is not null) return found;
        }
        return null;
    }

    internal static ForceNode? FindForceDeep(ForceNode force, string forceId)
    {
        if (force.Id == forceId) return force;
        foreach (var child in force.Forces)
        {
            var found = FindForceDeep(child, forceId);
            if (found is not null) return found;
        }
        return null;
    }

    /// <summary>
    /// Recursively searches for a selection node by ID within a force's selection tree
    /// (including selections in child forces).
    /// </summary>
    internal static SelectionNode? FindSelectionDeep(ForceNode force, string selectionId)
    {
        foreach (var sel in force.Selections)
        {
            var found = FindSelectionDeep(sel, selectionId);
            if (found is not null) return found;
        }
        foreach (var childForce in force.Forces)
        {
            var found = FindSelectionDeep(childForce, selectionId);
            if (found is not null) return found;
        }
        return null;
    }

    internal static SelectionNode? FindSelectionDeep(SelectionNode sel, string selectionId)
    {
        if (sel.Id == selectionId) return sel;
        foreach (var child in sel.Selections)
        {
            var found = FindSelectionDeep(child, selectionId);
            if (found is not null) return found;
        }
        return null;
    }

    internal static ForceNode FindForceDeepRequired(RosterNode roster, string forceId)
        => FindForceDeep(roster, forceId)
           ?? throw new ArgumentException($"Force '{forceId}' not found in roster.", nameof(forceId));

    internal static SelectionNode FindSelectionDeepRequired(ForceNode force, string forceId, string selectionId)
        => FindSelectionDeep(force, selectionId)
           ?? throw new ArgumentException(
               $"Selection '{selectionId}' not found in force '{forceId}'.", nameof(selectionId));

    /// <summary>
    /// Finds the parent selection that directly contains a child selection with the given ID.
    /// Returns null if the selection is at force root level.
    /// </summary>
    private static SelectionNode? FindParentSelection(ForceNode force, string selectionId)
    {
        foreach (var sel in force.Selections)
        {
            var parent = FindParentSelection(sel, selectionId);
            if (parent is not null) return parent;
        }
        foreach (var childForce in force.Forces)
        {
            var parent = FindParentSelection(childForce, selectionId);
            if (parent is not null) return parent;
        }
        return null;
    }

    private static SelectionNode? FindParentSelection(SelectionNode current, string selectionId)
    {
        foreach (var child in current.Selections)
        {
            if (child.Id == selectionId) return current;
            var parent = FindParentSelection(child, selectionId);
            if (parent is not null) return parent;
        }
        return null;
    }

    // ──────────────────────────────────────────────────────────────────────
    //  ID regeneration for duplication
    // ──────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Creates a deep copy of a selection subtree with all IDs regenerated.
    /// </summary>
    private static SelectionNode RegenerateIds(SelectionNode node)
    {
        var newChildren = NodeList.Create(node.Selections.Select(RegenerateIds));
        return node
            .WithId(NodeFactory.NewId())
            .WithSelections(node.Selections.WithNodes(newChildren));
    }

    /// <summary>
    /// Creates a deep copy of a force subtree with all IDs regenerated
    /// (force IDs, selection IDs, and child force IDs).
    /// </summary>
    private static ForceNode RegenerateIds(ForceNode node)
    {
        var newSelections = NodeList.Create(node.Selections.Select(RegenerateIds));
        var newChildForces = NodeList.Create(node.Forces.Select(RegenerateIds));
        return node
            .WithId(NodeFactory.NewId())
            .WithSelections(node.Selections.WithNodes(newSelections))
            .WithForces(node.Forces.WithNodes(newChildForces));
    }

    // ──────────────────────────────────────────────────────────────────────
    //  Symbol-based tree traversal (for ID-based operations)
    //  These are O(n) linear scans — acceptable for typical roster sizes
    //  (tens of forces, hundreds of selections). If rosters grow significantly
    //  larger, consider building an ID-to-symbol index.
    // ──────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Finds force and selection symbols for a given force/selection ID pair.
    /// Returns nulls for any part not found or when roster symbol is unavailable.
    /// </summary>
    private static SelectionContext FindSelectionContext(
        IRosterSymbol? rosterSymbol, string forceId, string selectionId)
    {
        if (rosterSymbol is null)
            return default;
        var forceSymbol = FindForceSymbolDeep(rosterSymbol, forceId);
        if (forceSymbol is null)
            return default;
        var selSymbol = FindSelectionSymbolDeep(forceSymbol, selectionId);
        return new(forceSymbol, selSymbol);
    }

    private readonly record struct SelectionContext(
        IForceSymbol? Force,
        ISelectionSymbol? Selection)
    {
        /// <summary>
        /// The parent selection, or null if this is a force-level selection.
        /// </summary>
        public ISelectionSymbol? ParentSelection =>
            Selection?.ContainingSymbol as ISelectionSymbol;
    }

    private static IForceSymbol? FindForceSymbolDeep(IRosterSymbol roster, string forceId)
    {
        foreach (var force in roster.Forces)
        {
            var found = FindForceSymbolDeep(force, forceId);
            if (found is not null) return found;
        }
        return null;
    }

    private static IForceSymbol? FindForceSymbolDeep(IForceSymbol force, string forceId)
    {
        if (force.Id == forceId) return force;
        foreach (var child in force.Forces)
        {
            var found = FindForceSymbolDeep(child, forceId);
            if (found is not null) return found;
        }
        return null;
    }

    private static ISelectionSymbol? FindSelectionSymbolDeep(IForceSymbol force, string selectionId)
    {
        foreach (var sel in force.Selections)
        {
            var found = FindSelectionSymbolDeep(sel, selectionId);
            if (found is not null) return found;
        }
        foreach (var childForce in force.Forces)
        {
            var found = FindSelectionSymbolDeep(childForce, selectionId);
            if (found is not null) return found;
        }
        return null;
    }

    private static ISelectionSymbol? FindSelectionSymbolDeep(ISelectionSymbol sel, string selectionId)
    {
        if (sel.Id == selectionId) return sel;
        foreach (var child in sel.Selections)
        {
            var found = FindSelectionSymbolDeep(child, selectionId);
            if (found is not null) return found;
        }
        return null;
    }

    // ──────────────────────────────────────────────────────────────────────
    //  ID-based force operations
    // ──────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Adds a top-level force to the roster and auto-selects root entries with
    /// min constraints. Returns a <see cref="MutationResult"/> containing the
    /// new force ID and auto-created selections map.
    /// </summary>
    public MutationResult AddForceById(
        RosterState state,
        IForceEntrySymbol forceEntry,
        ICatalogueSymbol catalogue)
    {
        // Create the force (reuse existing logic)
        state = AddForce(state, forceEntry, catalogue);
        var roster = state.RosterRequired;
        var newForce = roster.Forces[^1];
        var forceId = newForce.Id!;

        // Auto-select root entries with min constraints, filtered by force categories
        var forceCategoryIds = GetForceCategoryIds(forceEntry);
        state = AutoSelectRootEntries(state, roster.Forces.Count - 1, catalogue, forceCategoryIds);

        // Build selections map from the force's final state
        var finalForce = state.RosterRequired.Forces[^1];
        var selections = MutationResult.CollectSelectionMapFromForce(finalForce);

        return new MutationResult(state)
        {
            ForceId = forceId,
            Selections = selections
        };
    }

    /// <summary>
    /// Adds a child force nested inside an existing force. Does NOT auto-select
    /// root entries (matching BattleScribe behavior for nested forces).
    /// </summary>
    public MutationResult AddChildForceById(
        RosterState state,
        string parentForceId,
        IForceEntrySymbol forceEntry,
        ICatalogueSymbol catalogue)
    {
        var roster = state.RosterRequired;
        var parentForce = FindForceDeepRequired(roster, parentForceId);

        var forceEntryDecl = forceEntry.GetDeclaration()
            ?? throw new InvalidOperationException(
                $"Force entry '{forceEntry.Id}' has no declaration.");

        var catalogueDecl = catalogue.GetDeclaration() as CatalogueBaseNode;
        var childForce = NodeFactory.Force(forceEntryDecl, catalogueDecl);

        var categories = BuildForceCategories(forceEntry);
        if (categories.Length > 0)
        {
            childForce = childForce.AddCategories(categories);
        }

        var updatedParent = parentForce.AddForces(childForce);
        var newRoster = roster.Replace(parentForce, _ => updatedParent).WithUpdatedCostTotals();

        return new MutationResult(state.ReplaceRoster(newRoster))
        {
            ForceId = childForce.Id
        };
    }

    /// <summary>
    /// Removes a force by ID from anywhere in the roster's force tree.
    /// </summary>
    public RosterState RemoveForceById(RosterState state, string forceId)
    {
        var roster = state.RosterRequired;
        var force = FindForceDeepRequired(roster, forceId);
        var newRoster = roster.Remove(force).WithUpdatedCostTotals();
        return state.ReplaceRoster(newRoster);
    }

    /// <summary>
    /// Duplicates a top-level force. Returns the new force's ID.
    /// </summary>
    public MutationResult DuplicateForceById(RosterState state, string forceId)
    {
        var roster = state.RosterRequired;
        for (var i = 0; i < roster.Forces.Count; i++)
        {
            if (roster.Forces[i].Id == forceId)
            {
                var duplicate = RegenerateIds(roster.Forces[i]);
                var newRoster = roster.AddForces(duplicate).WithUpdatedCostTotals();
                var newState = state.ReplaceRoster(newRoster);
                return new MutationResult(newState)
                {
                    ForceId = duplicate.Id
                };
            }
        }

        throw new ArgumentException(
            $"Force '{forceId}' not found at top level for duplication.", nameof(forceId));
    }

    // ──────────────────────────────────────────────────────────────────────
    //  ID-based selection operations
    // ──────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Selects an entry by ID and adds it to the specified force (by ID, any nesting depth).
    /// Resolves the entry from the force's catalogue's available entries.
    /// </summary>
    public MutationResult SelectEntryById(
        RosterState state,
        string forceId,
        string entryId)
    {
        var roster = state.RosterRequired;
        var force = FindForceDeepRequired(roster, forceId);

        var catalogue = ResolveForceCatalogue(state, force);
        var available = EntryResolver.GetAvailableEntries(catalogue);
        var avail = EntryResolver.FindByEntryId(available, entryId);

        var selectionNode = CreateSelectionWithAutoChildren(avail.Symbol, avail.SourceGroup, avail.LinkPrefix);
        var newForce = force.AddSelections(selectionNode);
        var newRoster = roster.Replace(force, _ => newForce).WithUpdatedCostTotals();
        var newState = state.ReplaceRoster(newRoster);

        return new MutationResult(newState)
        {
            SelectionId = selectionNode.Id,
            Selections = MutationResult.CollectSelectionMap(selectionNode)
        };
    }

    /// <summary>
    /// Selects a child entry by ID and nests it under an existing selection (by ID, any nesting depth).
    /// Resolves the parent selection's source entry symbol to enumerate child entries.
    /// </summary>
    public MutationResult SelectChildEntryById(
        RosterState state,
        string forceId,
        string parentSelectionId,
        string entryId)
    {
        var roster = state.RosterRequired;
        var force = FindForceDeepRequired(roster, forceId);
        var parentSel = FindSelectionDeepRequired(force, forceId, parentSelectionId);

        // Resolve the parent selection's source entry symbol from the current compilation.
        var rosterSymbol = state.RosterSymbol
            ?? throw new InvalidOperationException("No roster symbol found in state.");
        var forceSymbol = FindForceSymbolDeep(rosterSymbol, forceId)
            ?? throw new InvalidOperationException($"Force symbol '{forceId}' not found.");
        var selectionSymbol = FindSelectionSymbolDeep(forceSymbol, parentSelectionId)
            ?? throw new InvalidOperationException(
                $"Selection symbol '{parentSelectionId}' not found in force '{forceId}'.");

        var parentEntry = selectionSymbol.SourceEntry;

        // Derive the link prefix accumulated up to the parent entry.
        // The parent's entryId is a "linkId::targetId" (or "l1::l2::target") path;
        // strip the last segment to recover the prefix used to reach parentEntry.
        var parentEntryId = parentSel.EntryId ?? "";
        var lastSep = parentEntryId.LastIndexOf("::", StringComparison.Ordinal);
        var linkPrefix = lastSep >= 0 ? parentEntryId[..lastSep] : "";

        var childEntries = EntryResolver.GetChildEntries(parentEntry, linkPrefix);
        var childAvail = EntryResolver.FindByEntryId(childEntries, entryId);

        // isDuplicate: if the child entry has only collective/hidden children,
        // repeated selectChildEntry increments the existing child's number.
        // Use the computed composite entryId for comparison, as stored selections
        // use the composite form (e.g., "linkId::targetId") while the input entryId
        // may be in raw form.
        var compositeEntryId = EntryResolver.ComputeCompositeEntryId(childAvail);
        if (IsNumberIncrementEntry(childAvail.Symbol))
        {
            foreach (var existingChild in parentSel.Selections)
            {
                if (existingChild.EntryId == compositeEntryId)
                {
                    // For collective entries, increment by parent's number (one per model).
                    var increment = IsEntryCollective(childAvail.Symbol) ? parentSel.Number : 1;
                    var newNumber = existingChild.Number + increment;
                    var newSel = ScaleChildSelections(existingChild, existingChild.Number, newNumber)
                        .WithNumber(newNumber);
                    var newParentInc = parentSel.Replace(existingChild, _ => newSel);
                    var newRosterInc = roster.Replace(parentSel, _ => newParentInc).WithUpdatedCostTotals();
                    return new MutationResult(state.ReplaceRoster(newRosterInc))
                    {
                        SelectionId = newSel.Id,
                        Selections = MutationResult.CollectSelectionMap(newSel)
                    };
                }
            }
        }

        // If a child with this entryId was already auto-selected to satisfy a min constraint,
        // return it as-is (no-op) instead of creating a duplicate.
        var entryMin = GetMinSelectionCount(childAvail.Symbol);
        if (entryMin >= 1)
        {
            foreach (var existingChild in parentSel.Selections)
            {
                if (existingChild.EntryId == compositeEntryId && existingChild.Number <= entryMin)
                {
                    return new MutationResult(state)
                    {
                        SelectionId = existingChild.Id,
                        Selections = MutationResult.CollectSelectionMap(existingChild)
                    };
                }
            }
        }

        var childSelectionNode = CreateSelectionWithAutoChildren(childAvail.Symbol, childAvail.SourceGroup, childAvail.LinkPrefix);

        // Collective child: inherit parent's number.
        if (IsEntryCollective(childAvail.Symbol) && parentSel.Number > 1)
        {
            var inheritedNumber = parentSel.Number;
            childSelectionNode = ScaleChildSelections(childSelectionNode, childSelectionNode.Number, inheritedNumber)
                .WithNumber(inheritedNumber);
        }

        var newParent = parentSel.AddSelections(childSelectionNode);
        var newRoster = roster.Replace(parentSel, _ => newParent).WithUpdatedCostTotals();
        var newState = state.ReplaceRoster(newRoster);

        return new MutationResult(newState)
        {
            SelectionId = childSelectionNode.Id,
            Selections = MutationResult.CollectSelectionMap(childSelectionNode)
        };
    }

    /// <summary>
    /// Removes a selection by ID from a force (any nesting depth).
    /// For collective entries, removes one per model (subtracts parent's number).
    /// If the resulting count is zero or below, removes the node entirely.
    /// </summary>
    public RosterState DeselectSelectionById(
        RosterState state,
        string forceId,
        string selectionId)
    {
        var roster = state.RosterRequired;
        var force = FindForceDeepRequired(roster, forceId);
        var selNode = FindSelectionDeepRequired(force, forceId, selectionId);

        var ctx = FindSelectionContext(state.RosterSymbol, forceId, selectionId);
        if (ctx.Selection is not null && IsEntryCollective(ctx.Selection.SourceEntry))
        {
            var parentNumber = ctx.ParentSelection?.SelectedCount ?? 0;
            if (parentNumber > 0)
            {
                var newNumber = selNode.Number - parentNumber;
                if (newNumber > 0)
                {
                    var newSelNode = ScaleChildSelections(selNode, selNode.Number, newNumber)
                        .WithUpdatedNumberAndCosts(newNumber);
                    var newRoster = roster.Replace(selNode, _ => newSelNode).WithUpdatedCostTotals();
                    return state.ReplaceRoster(newRoster);
                }
            }
        }

        var removedRoster = roster.Remove(selNode).WithUpdatedCostTotals();
        return state.ReplaceRoster(removedRoster);
    }

    /// <summary>
    /// Duplicates a selection by ID within a force. The duplicate is placed
    /// alongside the original (same parent), with regenerated IDs throughout.
    /// </summary>
    public MutationResult DuplicateSelectionById(
        RosterState state,
        string forceId,
        string selectionId)
    {
        var roster = state.RosterRequired;
        var force = FindForceDeepRequired(roster, forceId);
        var selNode = FindSelectionDeepRequired(force, forceId, selectionId);
        var duplicate = RegenerateIds(selNode);

        // Find the parent that contains this selection and add the duplicate alongside it
        var parentSel = FindParentSelection(force, selectionId);
        RosterNode newRoster;
        if (parentSel is not null)
        {
            var newParent = parentSel.AddSelections(duplicate);
            newRoster = roster.Replace(parentSel, _ => newParent).WithUpdatedCostTotals();
        }
        else
        {
            // Selection is at force root level
            var newForce = force.AddSelections(duplicate);
            newRoster = roster.Replace(force, _ => newForce).WithUpdatedCostTotals();
        }

        return new MutationResult(state.ReplaceRoster(newRoster))
        {
            SelectionId = duplicate.Id
        };
    }

    /// <summary>
    /// Sets the selection count by ID. For collective entries, the count is
    /// per-model (multiplied by the parent's number). When any selection's
    /// number changes, all children scale proportionally.
    /// </summary>
    public RosterState SetSelectionCountById(
        RosterState state,
        string forceId,
        string selectionId,
        int count)
    {
        var roster = state.RosterRequired;
        var force = FindForceDeepRequired(roster, forceId);
        var selNode = FindSelectionDeepRequired(force, forceId, selectionId);

        var oldNumber = selNode.Number;
        var actualNumber = count;

        var ctx = FindSelectionContext(state.RosterSymbol, forceId, selectionId);
        if (ctx.Selection is not null && IsEntryCollective(ctx.Selection.SourceEntry)
            && ctx.ParentSelection is { } parentSel)
        {
            actualNumber = count * parentSel.SelectedCount;
        }

        // Scale children proportionally when the number changes.
        var newSelNode = ScaleChildSelections(selNode, oldNumber, actualNumber)
            .WithUpdatedNumberAndCosts(actualNumber);
        var newRoster = roster.Replace(selNode, _ => newSelNode).WithUpdatedCostTotals();
        return state.ReplaceRoster(newRoster);
    }

    /// <summary>
    /// Sets customization (custom name/notes) on a force, selection, or category by ID.
    /// Categories only support <paramref name="customNotes"/>; passing a non-null
    /// <paramref name="customName"/> when <paramref name="categoryEntryId"/> is set throws.
    /// </summary>
    public RosterState SetCustomizationById(
        RosterState state,
        string forceId,
        string? selectionId,
        string? categoryEntryId,
        string? customName,
        string? customNotes)
    {
        var roster = state.RosterRequired;
        var force = FindForceDeepRequired(roster, forceId);

        RosterNode newRoster;
        if (categoryEntryId is not null)
        {
            if (customName is not null)
                throw new ArgumentException(
                    "Categories do not support custom names. Only customNotes can be set on a category.",
                    nameof(customName));
            if (selectionId is not null)
            {
                var selNode = FindSelectionDeepRequired(force, forceId, selectionId);
                var catNode = selNode.Categories.FirstOrDefault(c => c.EntryId == categoryEntryId)
                    ?? throw new InvalidOperationException($"Category '{categoryEntryId}' not found.");
                var newCat = catNode;
                if (customNotes is not null) newCat = newCat.WithCustomNotes(customNotes);
                newRoster = roster.Replace(catNode, _ => newCat);
            }
            else
            {
                var catNode = force.Categories.FirstOrDefault(c => c.EntryId == categoryEntryId)
                    ?? throw new InvalidOperationException($"Category '{categoryEntryId}' not found.");
                var newCat = catNode;
                if (customNotes is not null) newCat = newCat.WithCustomNotes(customNotes);
                newRoster = roster.Replace(catNode, _ => newCat);
            }
        }
        else if (selectionId is not null)
        {
            var selNode = FindSelectionDeepRequired(force, forceId, selectionId);
            var newSel = selNode;
            if (customName is not null) newSel = newSel.WithCustomName(customName);
            if (customNotes is not null) newSel = newSel.WithCustomNotes(customNotes);
            newRoster = roster.Replace(selNode, _ => newSel);
        }
        else
        {
            var newForce = force;
            if (customName is not null) newForce = newForce.WithCustomName(customName);
            if (customNotes is not null) newForce = newForce.WithCustomNotes(customNotes);
            newRoster = roster.Replace(force, _ => newForce);
        }

        return state.ReplaceRoster(newRoster);
    }

    // ──────────────────────────────────────────────────────────────────────
    //  Root entry auto-selection
    // ──────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Auto-selects root entries that have min constraints on force/parent scope.
    /// Called by <see cref="AddForceById"/> after adding a force to the roster.
    /// </summary>
    private RosterState AutoSelectRootEntries(
        RosterState state,
        int forceIndex,
        ICatalogueSymbol catalogue,
        HashSet<string> forceCategoryIds)
    {
        var available = EntryResolver.GetAvailableEntries(catalogue);
        var autoSelectedGroups = new HashSet<string>(StringComparer.Ordinal);

        foreach (var avail in available)
        {
            var effectiveEntry = avail.Symbol.IsReference
                ? avail.Symbol.ReferencedEntry ?? avail.Symbol
                : avail.Symbol;

            // Skip entries whose primary category is not in the force's categories.
            if (forceCategoryIds is not null && !EntryMatchesForceCategories(effectiveEntry, forceCategoryIds))
                continue;

            var minCount = GetMinAutoSelectCount(effectiveEntry);

            if (minCount < 1 && avail.SourceGroup is { Id: { } groupId } group)
            {
                if (!autoSelectedGroups.Contains(groupId))
                {
                    var groupMin = GetGroupMinAutoSelectCount(group);
                    if (groupMin >= 1 && IsDefaultAutoSelectEntry(avail, available, group))
                    {
                        minCount = groupMin;
                        autoSelectedGroups.Add(groupId);
                    }
                }
            }

            if (minCount < 1) continue;

            for (int i = 0; i < minCount; i++)
            {
                state = SelectEntry(state, forceIndex, avail.Symbol, avail.SourceGroup);
            }
        }

        return state;
    }

    /// <summary>
    /// Collects the set of category entry IDs declared on a force entry.
    /// Returns an empty set if the force has no categories (meaning no filtering needed for
    /// uncategorized entries, but categorized entries should NOT auto-select).
    /// </summary>
    private static HashSet<string> GetForceCategoryIds(IForceEntrySymbol forceEntry)
    {
        if (forceEntry.Categories.IsEmpty)
            return new HashSet<string>(StringComparer.Ordinal);

        var ids = new HashSet<string>(StringComparer.Ordinal);
        foreach (var cat in forceEntry.Categories)
        {
            var target = cat.ReferencedEntry ?? cat;
            if (target.Id is { } id)
                ids.Add(id);
        }
        return ids;
    }

    /// <summary>
    /// Checks if a root entry's primary category is among the force's declared categories.
    /// Entries without a primary category are allowed in any force.
    /// When the force has no categories, only uncategorized entries are allowed.
    /// </summary>
    private static bool EntryMatchesForceCategories(
        ISelectionEntryContainerSymbol entry, HashSet<string> forceCategoryIds)
    {
        var primaryCat = entry.PrimaryCategory;
        if (primaryCat is null)
            return true; // uncategorized entries are available everywhere

        var catTarget = primaryCat.ReferencedEntry ?? primaryCat;
        return catTarget.Id is { } catId && forceCategoryIds.Contains(catId);
    }

    /// <summary>
    /// Gets the minimum auto-select count from an entry's constraints.
    /// Uses the raw constraint declarations (not the symbol API) to match
    /// the auto-select behavior that checks <c>type=min, field=selections,
    /// scope=parent|force, value≥1</c>.
    /// </summary>
    private static int GetMinAutoSelectCount(ISelectionEntryContainerSymbol entry)
    {
        foreach (var constraint in entry.Constraints)
        {
            var decl = constraint.GetDeclaration();
            if (decl is null) continue;
            if (decl.Type != ConstraintKind.Minimum) continue;
            if (decl.Field is not "selections") continue;
            if (decl.Scope is not ("parent" or "force")) continue;
            if (decl.IsValuePercentage) continue;
            var value = (int)decl.Value;
            if (value >= 1) return value;
        }
        return 0;
    }

    private static int GetGroupMinAutoSelectCount(ISelectionEntryGroupSymbol group)
    {
        foreach (var constraint in group.Constraints)
        {
            var decl = constraint.GetDeclaration();
            if (decl is null) continue;
            if (decl.Type != ConstraintKind.Minimum) continue;
            if (decl.Field is not "selections") continue;
            if (decl.Scope is not ("parent" or "force")) continue;
            if (decl.IsValuePercentage) continue;
            var value = (int)decl.Value;
            if (value >= 1) return value;
        }
        return 0;
    }

    /// <summary>
    /// Checks if an available entry is the default entry for its group during root auto-selection.
    /// Unlike <see cref="IsDefaultEntryForGroup"/> (which takes a raw entry symbol),
    /// this overload works with <see cref="AvailableEntry"/> and the full available list
    /// to count group members from the flattened list rather than the group's direct children.
    /// </summary>
    private static bool IsDefaultAutoSelectEntry(
        AvailableEntry avail,
        IReadOnlyList<AvailableEntry> allEntries,
        ISelectionEntryGroupSymbol group)
    {
        var defaultEntry = group.DefaultSelectionEntry;
        if (defaultEntry is not null)
        {
            return MatchesDefaultEntry(avail.Symbol, defaultEntry);
        }

        var count = 0;
        foreach (var entry in allEntries)
        {
            if (entry.SourceGroup == group) count++;
            if (count > 1) return false;
        }
        return count == 1;
    }
}
