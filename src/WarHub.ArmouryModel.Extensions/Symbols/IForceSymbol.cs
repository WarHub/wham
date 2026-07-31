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
    /// The force entry with modifiers applied in this roster context.
    /// Effective resources (profiles, rules) are available via <see cref="IEntrySymbol.Resources"/>
    /// on the returned symbol. When modifier evaluation is not available,
    /// returns the declared <see cref="IEntryInstanceSymbol.SourceEntry"/> as-is.
    /// </summary>
    IForceEntrySymbol EffectiveSourceEntry { get; }
}
