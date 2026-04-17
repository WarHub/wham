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
    private readonly EntryResolver _entryResolver = new();

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
        return _entryResolver.GetAvailableEntries(catalogue);
    }

    /// <summary>
    /// Gets the available child entries for a specific entry symbol.
    /// Delegates to <see cref="EntryResolver.GetChildEntries"/>.
    /// </summary>
    public IReadOnlyList<AvailableEntry> GetChildEntries(ISelectionEntryContainerSymbol entry)
    {
        return _entryResolver.GetChildEntries(entry);
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
        // Create a structural copy. The immutable node framework ensures a fresh tree
        // is built when we re-add the same shape. Future work: assign new IDs throughout
        // the subtree to guarantee uniqueness.
        var newForce = force.AddSelections(original);
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
    /// </summary>
    private SelectionNode CreateSelectionWithAutoChildren(
        ISelectionEntryContainerSymbol entry,
        ISelectionEntryGroupSymbol? sourceGroup)
    {
        var selectionNode = CreateSelectionNode(entry, sourceGroup);

        // Auto-select child entries that have a minimum constraint ≥ 1.
        var childEntries = _entryResolver.GetChildEntries(entry);
        foreach (var child in childEntries)
        {
            var autoCount = GetMinSelectionCount(child.Symbol);
            if (autoCount < 1)
            {
                continue;
            }

            // Create one child selection with Number set to the min count.
            var childSelection = CreateSelectionWithAutoChildren(child.Symbol, child.SourceGroup);
            if (autoCount > 1)
            {
                childSelection = childSelection.WithNumber(autoCount);
            }
            selectionNode = selectionNode.AddSelections(childSelection);
        }

        return selectionNode;
    }

    /// <summary>
    /// Creates a <see cref="SelectionNode"/> from an <see cref="ISelectionEntryContainerSymbol"/>.
    /// Resolves the backing <see cref="SelectionEntryNode"/> declaration through links,
    /// then populates costs and categories.
    /// </summary>
    private static SelectionNode CreateSelectionNode(
        ISelectionEntryContainerSymbol entry,
        ISelectionEntryGroupSymbol? sourceGroup)
    {
        var entryDecl = ResolveToEntryDeclaration(entry)
            ?? throw new ArgumentException(
                $"Cannot resolve entry '{entry.Name}' (ID: {entry.Id}) to a SelectionEntryNode declaration.",
                nameof(entry));

        // Build entryId path matching BattleScribe format:
        // For entry links, use "linkId::targetId" so the binder resolves
        // SourceEntryPath = [link, target] with SourceEntry = target.
        // For direct entries, use just the entry's ID.
        var entryId = BuildEntryIdPath(entry);
        var entryGroupId = sourceGroup?.Id;

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
    /// returns just the entry's own ID.
    /// </summary>
    private static string BuildEntryIdPath(ISelectionEntryContainerSymbol entry)
    {
        if (entry.ReferencedEntry is { Id: { } targetId })
        {
            var linkId = entry.Id ?? "";
            return $"{linkId}::{targetId}";
        }
        return entry.Id ?? "";
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
}
