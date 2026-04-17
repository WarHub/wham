namespace WarHub.ArmouryModel.Concrete;

/// <summary>
/// Wraps an <see cref="ISelectionEntryContainerSymbol"/> with effective values
/// computed by applying modifiers in a roster context.
/// Analogous to Roslyn's <c>SubstitutedNamedTypeSymbol</c>.
/// <para>
/// Adds selection-specific overrides: <see cref="Categories"/>, <see cref="PrimaryCategory"/>.
/// </para>
/// </summary>
internal sealed class EffectiveSelectionEntrySymbol : EffectiveContainerEntrySymbol, ISelectionEntryContainerSymbol
{
    public EffectiveSelectionEntrySymbol(
        ISelectionEntryContainerSymbol original,
        string effectiveName,
        bool effectiveIsHidden,
        IReadOnlyDictionary<string, decimal> effectiveConstraintValues,
        ImmutableArray<IResourceEntrySymbol> effectiveResources,
        ImmutableArray<ICategoryEntrySymbol> effectiveCategories,
        ICategoryEntrySymbol? effectivePrimaryCategory,
        IPublicationReferenceSymbol? effectivePublicationReference)
        : base(original, effectiveName, effectiveIsHidden, effectiveConstraintValues, effectiveResources, effectivePublicationReference)
    {
        OriginalEntry = original;
        Categories = effectiveCategories;
        PrimaryCategory = effectivePrimaryCategory;
    }

    /// <summary>
    /// The declared (unmodified) selection entry this effective symbol wraps.
    /// </summary>
    public ISelectionEntryContainerSymbol OriginalEntry { get; }

    protected override IContainerEntrySymbol OriginalContainerEntry => OriginalEntry;

    // Selection-specific overrides
    public ImmutableArray<ICategoryEntrySymbol> Categories { get; }
    public ICategoryEntrySymbol? PrimaryCategory { get; }

    // Delegated from ISelectionEntryContainerSymbol
    public ImmutableArray<ISelectionEntryContainerSymbol> ChildSelectionEntries => OriginalEntry.ChildSelectionEntries;

    // ISelectionEntryContainerSymbol.ReferencedEntry (explicit for `new` member)
    ISelectionEntryContainerSymbol? ISelectionEntryContainerSymbol.ReferencedEntry => null;
}
