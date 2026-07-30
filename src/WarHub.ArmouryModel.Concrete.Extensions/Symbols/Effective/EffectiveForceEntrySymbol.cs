namespace WarHub.ArmouryModel.Concrete;

/// <summary>
/// Wraps an <see cref="IForceEntrySymbol"/> with effective (modifier-applied) values
/// computed in a roster context. Extends <see cref="EffectiveContainerEntrySymbol"/>
/// with force-specific members: <see cref="ChildForces"/> and <see cref="Categories"/>.
/// </summary>
internal sealed class EffectiveForceEntrySymbol : EffectiveContainerEntrySymbol, IForceEntrySymbol
{
    public EffectiveForceEntrySymbol(
        IForceEntrySymbol original,
        string effectiveName,
        bool effectiveIsHidden,
        IReadOnlyDictionary<string, decimal> effectiveConstraintValues,
        ImmutableArray<IResourceEntrySymbol> effectiveResources,
        IPublicationReferenceSymbol? effectivePublicationReference)
        : base(original, effectiveName, effectiveIsHidden, effectiveConstraintValues, effectiveResources, effectivePublicationReference)
    {
        OriginalForceEntry = original;
    }

    /// <summary>
    /// The declared (unmodified) force entry this effective symbol wraps.
    /// </summary>
    public IForceEntrySymbol OriginalForceEntry { get; }

    protected override IContainerEntrySymbol OriginalContainerEntry => OriginalForceEntry;

    // Delegated from IForceEntrySymbol
    public ImmutableArray<IForceEntrySymbol> ChildForces => OriginalForceEntry.ChildForces;
    public ImmutableArray<ICategoryEntrySymbol> Categories => OriginalForceEntry.Categories;
}
