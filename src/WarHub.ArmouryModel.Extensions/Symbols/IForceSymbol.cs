namespace WarHub.ArmouryModel;

/// <summary>
/// Force instance in a roster.
/// BS Force.
/// WHAM <see cref="Source.ForceNode" />.
/// </summary>
public interface IForceSymbol : ISelectionContainerSymbol, IForceContainerSymbol
{
    new IForceEntrySymbol SourceEntry { get; }

    ICatalogueReferenceSymbol CatalogueReference { get; }

    // TODO catalogue reference: id, name, revision?

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
}
