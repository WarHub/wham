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
        var compilation = catalogCompilation.AddSourceTrees(rosterTree);
        return new RosterState(compilation);
    }

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

        // Create the force node. NodeFactory.Force requires the ForceEntryNode
        // to be a descendant of a CatalogueBaseNode (for catalogueId/name/revision),
        // which is guaranteed for declarations obtained from the symbol tree.
        var forceNode = NodeFactory.Force(forceEntryDecl);

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
        if ((uint)forceIndex >= (uint)roster.Forces.Count)
        {
            throw new ArgumentOutOfRangeException(
                nameof(forceIndex),
                forceIndex,
                $"Force index must be in [0, {roster.Forces.Count}). Roster contains {roster.Forces.Count} force(s).");
        }

        var forceToRemove = roster.Forces[forceIndex];
        var newRoster = roster.Remove(forceToRemove).WithUpdatedCostTotals();
        return state.ReplaceRoster(newRoster);
    }

    /// <summary>
    /// Adds a selection to a force based on the given entry symbol.
    /// Creates a <see cref="SelectionNode"/> with costs, categories, and auto-selected children.
    /// </summary>
    public RosterState SelectEntry(
        RosterState state,
        int forceIndex,
        ISelectionEntryContainerSymbol entry,
        ISelectionEntryGroupSymbol? sourceGroup = null)
    {
        var roster = state.RosterRequired;
        var force = roster.Forces[forceIndex];

        var selectionNode = BuildSelectionNode(entry, sourceGroup);
        selectionNode = AutoSelectChildren(selectionNode, entry);

        var newForce = force.AddSelections(selectionNode);
        var newRoster = ReplaceForce(roster, forceIndex, newForce).WithUpdatedCostTotals();
        return state.ReplaceRoster(newRoster);
    }

    /// <summary>
    /// Adds a child selection under an existing selection within a force.
    /// </summary>
    public RosterState SelectChildEntry(
        RosterState state,
        int forceIndex,
        int selectionIndex,
        ISelectionEntryContainerSymbol childEntry)
    {
        var roster = state.RosterRequired;
        var force = roster.Forces[forceIndex];
        var parentSelection = force.Selections[selectionIndex];

        var childNode = BuildSelectionNode(childEntry, sourceGroup: null);
        var newParent = parentSelection.AddSelections(childNode);
        var newForce = ReplaceSelection(force, selectionIndex, newParent);
        var newRoster = ReplaceForce(roster, forceIndex, newForce).WithUpdatedCostTotals();
        return state.ReplaceRoster(newRoster);
    }

    /// <summary>
    /// Removes a root-level selection from a force.
    /// </summary>
    public RosterState DeselectSelection(RosterState state, int forceIndex, int selectionIndex)
    {
        var roster = state.RosterRequired;
        var force = roster.Forces[forceIndex];
        var selectionToRemove = force.Selections[selectionIndex];

        var newForce = force.Remove(selectionToRemove);
        var newRoster = ReplaceForce(roster, forceIndex, newForce).WithUpdatedCostTotals();
        return state.ReplaceRoster(newRoster);
    }

    /// <summary>
    /// Duplicates a root-level selection within a force, including all children.
    /// The duplicate gets Number = 1.
    /// </summary>
    public RosterState DuplicateSelection(RosterState state, int forceIndex, int selectionIndex)
    {
        var roster = state.RosterRequired;
        var force = roster.Forces[forceIndex];
        var original = force.Selections[selectionIndex];

        // Deep copy: the node is immutable, so we can reuse it as-is but reset Number to 1.
        var duplicate = original.WithNumber(1);
        var newForce = force.AddSelections(duplicate);
        var newRoster = ReplaceForce(roster, forceIndex, newForce).WithUpdatedCostTotals();
        return state.ReplaceRoster(newRoster);
    }

    /// <summary>
    /// Sets the cost limit for a cost type on the roster.
    /// </summary>
    public RosterState SetCostLimit(RosterState state, string costTypeId, decimal value)
    {
        var roster = state.RosterRequired;
        var newLimits = roster.CostLimits
            .Select(cl => cl.TypeId == costTypeId ? cl.WithValue(value) : cl)
            .ToArray();
        var newRoster = roster.WithCostLimits(newLimits);
        return state.ReplaceRoster(newRoster);
    }

    // TODO Phase 4: EvaluateModifiers — apply IEffectSymbol / IConditionSymbol modifiers
    // TODO Phase 5: ValidateConstraints — evaluate IConstraintSymbol / IQuerySymbol rules

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

    /// <summary>
    /// Creates a <see cref="SelectionNode"/> from an entry symbol, including costs and categories.
    /// </summary>
    private static SelectionNode BuildSelectionNode(
        ISelectionEntryContainerSymbol entry,
        ISelectionEntryGroupSymbol? sourceGroup)
    {
        // Resolve the effective entry (follow reference links)
        var effectiveEntry = entry.IsReference ? entry.ReferencedEntry ?? entry : entry;

        // Get the entry declaration node for NodeFactory
        var entryDecl = effectiveEntry.GetEntryDeclaration();
        if (entryDecl is null && effectiveEntry is ISelectionEntryGroupSymbol groupSym)
        {
            // Groups need their declaration resolved differently
            var groupDecl = groupSym.GetEntryGroupDeclaration();
            return BuildGroupSelectionNode(groupSym, groupDecl, sourceGroup);
        }

        if (entryDecl is null)
        {
            throw new InvalidOperationException(
                $"Entry symbol '{entry.Name}' (Id={entry.Id}) has no backing declaration node.");
        }

        // Create base selection node
        var selNode = NodeFactory.Selection(entryDecl, effectiveEntry.Id)
            .WithNumber(1);

        // Add costs from the entry
        selNode = AddCosts(selNode, effectiveEntry);

        // Add categories from the entry and optionally from source group
        selNode = AddCategories(selNode, effectiveEntry, sourceGroup);

        return selNode;
    }

    /// <summary>
    /// Builds a selection node representing a selected group (used when a group is directly selectable).
    /// </summary>
    private static SelectionNode BuildGroupSelectionNode(
        ISelectionEntryGroupSymbol groupSym,
        SelectionEntryGroupNode? groupDecl,
        ISelectionEntryGroupSymbol? sourceGroup)
    {
        // Groups selected as entries get type "upgrade" and use the group's ID
        var selCore = new SelectionCore
        {
            Id = Guid.NewGuid().ToString(),
            Name = groupSym.Name,
            EntryId = groupSym.Id,
            Type = SelectionEntryKind.Upgrade,
            Number = 1,
        };
        var selNode = selCore.ToNode();

        // Add categories from group
        selNode = AddGroupCategories(selNode, groupSym, sourceGroup);

        return selNode;
    }

    /// <summary>
    /// Adds cost nodes to a selection from the entry's defined costs.
    /// </summary>
    private static SelectionNode AddCosts(SelectionNode selNode, ISelectionEntryContainerSymbol entry)
    {
        if (entry is not ISelectionEntrySymbol selEntry)
            return selNode;

        var costs = new List<CostNode>();
        foreach (var costSym in selEntry.Costs)
        {
            var costDecl = costSym.GetDeclaration();
            if (costDecl is not null)
            {
                costs.Add(costDecl);
            }
        }

        return costs.Count > 0 ? selNode.WithCosts(costs.ToArray()) : selNode;
    }

    /// <summary>
    /// Adds category nodes to a selection from the entry's category links.
    /// If <paramref name="sourceGroup"/> is provided, its categories are also inherited.
    /// </summary>
    private static SelectionNode AddCategories(
        SelectionNode selNode,
        ISelectionEntryContainerSymbol entry,
        ISelectionEntryGroupSymbol? sourceGroup)
    {
        var categories = new List<CategoryNode>();
        var addedIds = new HashSet<string>(StringComparer.Ordinal);

        // Entry's own categories
        foreach (var catSym in entry.Categories)
        {
            var targetEntry = catSym.ReferencedEntry ?? catSym;
            var catDecl = targetEntry.GetEntryDeclaration();
            if (catDecl is null) continue;

            var catNode = NodeFactory.Category(catDecl).WithPrimary(catSym.IsPrimaryCategory);
            categories.Add(catNode);
            addedIds.Add(catDecl.Id ?? "");
        }

        // Source group's categories (inherited, not duplicated)
        if (sourceGroup is not null)
        {
            foreach (var catSym in sourceGroup.Categories)
            {
                var targetEntry = catSym.ReferencedEntry ?? catSym;
                var catDecl = targetEntry.GetEntryDeclaration();
                if (catDecl is null) continue;
                if (!addedIds.Add(catDecl.Id ?? "")) continue;

                categories.Add(
                    NodeFactory.Category(catDecl).WithPrimary(catSym.IsPrimaryCategory));
            }
        }

        return categories.Count > 0 ? selNode.AddCategories([.. categories]) : selNode;
    }

    /// <summary>
    /// Adds category nodes to a group selection node.
    /// </summary>
    private static SelectionNode AddGroupCategories(
        SelectionNode selNode,
        ISelectionEntryGroupSymbol groupSym,
        ISelectionEntryGroupSymbol? sourceGroup)
    {
        var categories = new List<CategoryNode>();

        foreach (var catSym in groupSym.Categories)
        {
            var targetEntry = catSym.ReferencedEntry ?? catSym;
            var catDecl = targetEntry.GetEntryDeclaration();
            if (catDecl is null) continue;
            categories.Add(NodeFactory.Category(catDecl).WithPrimary(catSym.IsPrimaryCategory));
        }

        if (sourceGroup is not null && !ReferenceEquals(sourceGroup, groupSym))
        {
            var addedIds = new HashSet<string>(categories.Select(c => c.EntryId ?? ""), StringComparer.Ordinal);
            foreach (var catSym in sourceGroup.Categories)
            {
                var targetEntry = catSym.ReferencedEntry ?? catSym;
                var catDecl = targetEntry.GetEntryDeclaration();
                if (catDecl is null) continue;
                if (!addedIds.Add(catDecl.Id ?? "")) continue;
                categories.Add(NodeFactory.Category(catDecl).WithPrimary(catSym.IsPrimaryCategory));
            }
        }

        return categories.Count > 0 ? selNode.AddCategories([.. categories]) : selNode;
    }

    /// <summary>
    /// Auto-selects child entries that have min constraints requiring at least 1.
    /// This mirrors the legacy engine behavior of enforcing mandatory child selections.
    /// </summary>
    private static SelectionNode AutoSelectChildren(
        SelectionNode parentNode,
        ISelectionEntryContainerSymbol parentEntry)
    {
        var effectiveEntry = parentEntry.IsReference ? parentEntry.ReferencedEntry ?? parentEntry : parentEntry;

        foreach (var childSym in effectiveEntry.ChildSelectionEntries)
        {
            var effectiveChild = childSym.IsReference ? childSym.ReferencedEntry ?? childSym : childSym;

            // Check for min constraint requiring auto-selection
            var minCount = GetMinConstraintValue(effectiveChild);
            if (minCount < 1) continue;

            var childNode = BuildSelectionNode(childSym, sourceGroup: null);
            childNode = childNode.WithNumber(minCount);
            childNode = AutoSelectChildren(childNode, childSym);
            parentNode = parentNode.AddSelections(childNode);
        }

        return parentNode;
    }

    /// <summary>
    /// Gets the minimum selection count from constraints (scope=parent/force, field=selections, type=min).
    /// Returns 0 if no such constraint exists.
    /// Uses the constraint's backing <see cref="Source.ConstraintNode"/> declaration for raw field access.
    /// </summary>
    private static int GetMinConstraintValue(ISelectionEntryContainerSymbol entry)
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

    /// <summary>
    /// Replaces a force at the given index in the roster.
    /// </summary>
    private static RosterNode ReplaceForce(RosterNode roster, int forceIndex, ForceNode newForce)
    {
        var forces = roster.Forces.Select((f, i) => i == forceIndex ? newForce : f).ToArray();
        return roster.WithForces(forces);
    }

    /// <summary>
    /// Replaces a selection at the given index in a force.
    /// </summary>
    private static ForceNode ReplaceSelection(ForceNode force, int selectionIndex, SelectionNode newSelection)
    {
        var selections = force.Selections.Select((s, i) => i == selectionIndex ? newSelection : s).ToArray();
        return force.WithSelections(selections);
    }
}
