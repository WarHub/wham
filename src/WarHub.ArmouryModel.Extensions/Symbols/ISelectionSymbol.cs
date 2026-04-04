using WarHub.ArmouryModel.Source;

namespace WarHub.ArmouryModel;

/// <summary>
/// Selection instance in a roster.
/// BS Selection.
/// WHAM <see cref="Source.SelectionNode" />.
/// </summary>
public interface ISelectionSymbol : ISelectionContainerSymbol
{
    /// <summary>
    /// The entry ID string as stored in the roster (e.g., "linkId::targetId").
    /// This is the BattleScribe roster-level identity, distinct from
    /// <see cref="IEntryInstanceSymbol.SourceEntry"/>.<see cref="ISymbol.Id"/>.
    /// </summary>
    string? EntryId { get; }

    /// <summary>
    /// Selection count (number of times that selection is "taken").
    /// </summary>
    int SelectedCount { get; }

    SelectionEntryKind EntryKind { get; }

    new ISelectionEntrySymbol SourceEntry { get; }

    /// <summary>
    /// The source entry with modifiers applied in this roster context.
    /// When modifier evaluation is not available, returns the declared
    /// <see cref="SourceEntry"/> as-is.
    /// </summary>
    ISelectionEntryContainerSymbol EffectiveSourceEntry { get; }

    ICategorySymbol? PrimaryCategory { get; }

    ImmutableArray<ICategorySymbol> Categories { get; }

    /// <summary>
    /// Costs for this selection (with <see cref="SelectedCount"/> taken into account).
    /// Doesn't include costs of <see cref="ISelectionContainerSymbol.Selections"/>.
    /// </summary>
    ImmutableArray<ICostSymbol> Costs { get; }
}
