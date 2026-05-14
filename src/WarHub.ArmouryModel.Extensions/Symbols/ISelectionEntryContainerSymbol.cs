namespace WarHub.ArmouryModel;

/// <summary>
/// Recursive selection container.
/// BS SelectionEntry/SelectionEntryGroup/EntryLink
/// WHAM <see cref="Source.SelectionEntryNode" />.
/// </summary>
public interface ISelectionEntryContainerSymbol : IContainerEntrySymbol
{
    /// <summary>
    /// Whether this entry is collective (per-model semantics for number operations).
    /// </summary>
    bool IsCollective { get; }

    ICategoryEntrySymbol? PrimaryCategory { get; }

    ImmutableArray<ICategoryEntrySymbol> Categories { get; }

    ImmutableArray<ISelectionEntryContainerSymbol> ChildSelectionEntries { get; }

    new ISelectionEntryContainerSymbol? ReferencedEntry { get; }
}
