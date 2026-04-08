namespace WarHub.ArmouryModel;

/// <summary>
/// Recursive selection container.
/// BS SelectionEntry/SelectionEntryGroup/EntryLink
/// WHAM <see cref="Source.SelectionEntryNode" />.
/// </summary>
public interface ISelectionEntryContainerSymbol : IContainerEntrySymbol
{
    ICategoryEntrySymbol? PrimaryCategory { get; }

    ImmutableArray<ICategoryEntrySymbol> Categories { get; }

    ImmutableArray<ISelectionEntryContainerSymbol> ChildSelectionEntries { get; }

    new ISelectionEntryContainerSymbol? ReferencedEntry { get; }

    /// <summary>
    /// Profiles with modifier-applied values, collected from the entry's resource graph.
    /// Empty when effective entry computation is not available.
    /// </summary>
    ImmutableArray<IProfileSymbol> EffectiveProfiles => ImmutableArray<IProfileSymbol>.Empty;

    /// <summary>
    /// Rules with modifier-applied values, collected from the entry's resource graph.
    /// Empty when effective entry computation is not available.
    /// </summary>
    ImmutableArray<IRuleSymbol> EffectiveRules => ImmutableArray<IRuleSymbol>.Empty;
}
