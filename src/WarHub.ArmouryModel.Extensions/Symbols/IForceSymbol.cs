namespace WarHub.ArmouryModel;

/// <summary>
/// Force instance in a roster.
/// BS Force.
/// WHAM <see cref="Source.ForceNode" />.
/// </summary>
public interface IForceSymbol : ISelectionContainerSymbol, IForceContainerSymbol
{
    /// <summary>
    /// The force entry ID string from the roster.
    /// </summary>
    string? EntryId { get; }

    new IForceEntrySymbol SourceEntry { get; }

    ICatalogueReferenceSymbol CatalogueReference { get; }

    /// <summary>
    /// Categories declared in the <see cref="SourceEntry"/>.
    /// </summary>
    ImmutableArray<ICategorySymbol> Categories { get; }

    /// <summary>
    /// Publications provided by Catalogue.
    /// </summary>
    ImmutableArray<IPublicationSymbol> Publications { get; }

    /// <summary>
    /// Returns the effective (modifier-applied) version of a declared entry
    /// in this force's context. When modifier evaluation is not available,
    /// returns <paramref name="declaredEntry"/> as-is.
    /// </summary>
    ISelectionEntryContainerSymbol GetEffectiveEntry(
        ISelectionEntryContainerSymbol declaredEntry);

    /// <summary>
    /// Profiles with modifier-applied values, collected from the force entry's resource graph.
    /// Empty when effective entry computation is not available.
    /// </summary>
    ImmutableArray<IProfileSymbol> EffectiveProfiles => ImmutableArray<IProfileSymbol>.Empty;

    /// <summary>
    /// Rules with modifier-applied values, collected from the force entry's resource graph.
    /// Empty when effective entry computation is not available.
    /// </summary>
    ImmutableArray<IRuleSymbol> EffectiveRules => ImmutableArray<IRuleSymbol>.Empty;
}
