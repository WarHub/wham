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
    ImmutableArray<IEffectiveProfileSymbol> EffectiveProfiles => ImmutableArray<IEffectiveProfileSymbol>.Empty;

    /// <summary>
    /// Rules with modifier-applied values, collected from the entry's resource graph.
    /// Empty when effective entry computation is not available.
    /// </summary>
    ImmutableArray<IEffectiveRuleSymbol> EffectiveRules => ImmutableArray<IEffectiveRuleSymbol>.Empty;

    /// <summary>
    /// Effective page after applying page modifiers. Null when no page modifiers apply
    /// or effective entry computation is not available. Consumers should fall back to
    /// <see cref="IEntrySymbol.PublicationReference"/>.<see cref="IPublicationReferenceSymbol.Page"/>
    /// when this is null.
    /// </summary>
    string? EffectivePage => null;
}
