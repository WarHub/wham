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

    // TODO Phase 3: AddSelection, RemoveSelection, SetSelectionCount — selection lifecycle
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
}
