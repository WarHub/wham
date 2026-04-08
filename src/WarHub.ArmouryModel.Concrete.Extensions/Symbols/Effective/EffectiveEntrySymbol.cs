namespace WarHub.ArmouryModel.Concrete;

/// <summary>
/// Wraps an <see cref="ISelectionEntryContainerSymbol"/> with effective values
/// computed by applying modifiers in a roster context.
/// Analogous to Roslyn's <c>SubstitutedNamedTypeSymbol</c>.
/// <para>
/// Overridden members: <see cref="Name"/>, <see cref="IsHidden"/>,
/// <see cref="Constraints"/>, <see cref="Costs"/>,
/// <see cref="Categories"/>, <see cref="PrimaryCategory"/>.
/// </para>
/// All other members delegate to <see cref="OriginalEntry"/>.
/// </summary>
internal sealed class EffectiveEntrySymbol : ISelectionEntryContainerSymbol
{
    /// <summary>
    /// Creates an effective entry symbol from pre-computed modifier results.
    /// </summary>
    /// <param name="original">The declared (unmodified) entry symbol.</param>
    /// <param name="effectiveName">Name after applying modifiers.</param>
    /// <param name="effectiveIsHidden">Hidden flag after applying modifiers.</param>
    /// <param name="effectiveConstraintValues">
    /// Map of constraint ID → effective ReferenceValue. Constraints not in
    /// the dictionary keep their declared value.
    /// </param>
    /// <param name="effectiveResources">
    /// Flat collection of effective resources (profiles, rules, costs) already
    /// computed by the cache. Costs are extracted from this array for the
    /// <see cref="Costs"/> property.
    /// </param>
    /// <param name="effectiveCategories">Category entry symbols after applying modifiers.</param>
    /// <param name="effectivePrimaryCategory">Primary category after applying modifiers.</param>
    public EffectiveEntrySymbol(
        ISelectionEntryContainerSymbol original,
        string effectiveName,
        bool effectiveIsHidden,
        IReadOnlyDictionary<string, decimal> effectiveConstraintValues,
        ImmutableArray<IResourceEntrySymbol> effectiveResources,
        ImmutableArray<ICategoryEntrySymbol> effectiveCategories,
        ICategoryEntrySymbol? effectivePrimaryCategory,
        IPublicationReferenceSymbol? effectivePublicationReference)
    {
        OriginalEntry = original;
        Name = effectiveName;
        IsHidden = effectiveIsHidden;
        Categories = effectiveCategories;
        PrimaryCategory = effectivePrimaryCategory;
        Constraints = BuildEffectiveConstraints(original.Constraints, effectiveConstraintValues);
        Resources = effectiveResources;
        Costs = ExtractCosts(effectiveResources);
        PublicationReference = effectivePublicationReference;
    }

    /// <summary>
    /// The declared (unmodified) entry this effective symbol wraps.
    /// </summary>
    public ISelectionEntryContainerSymbol OriginalEntry { get; }

    // Overridden effective values
    public string Name { get; }
    public bool IsHidden { get; }
    public ImmutableArray<IConstraintSymbol> Constraints { get; }
    public ImmutableArray<ICostSymbol> Costs { get; }
    public ImmutableArray<ICategoryEntrySymbol> Categories { get; }
    public ICategoryEntrySymbol? PrimaryCategory { get; }
    public IPublicationReferenceSymbol? PublicationReference { get; }

    // Delegated from ISelectionEntryContainerSymbol
    public ImmutableArray<ISelectionEntryContainerSymbol> ChildSelectionEntries => OriginalEntry.ChildSelectionEntries;

    // ISelectionEntryContainerSymbol.ReferencedEntry (explicit for `new` member)
    ISelectionEntryContainerSymbol? ISelectionEntryContainerSymbol.ReferencedEntry => null;

    // Delegated from IContainerEntrySymbol
    public ContainerKind ContainerKind => OriginalEntry.ContainerKind;

    // Standalone effective values (not delegated — effective entry is never a link)
    public bool IsReference => false;
    public ImmutableArray<IEffectSymbol> Effects => ImmutableArray<IEffectSymbol>.Empty;
    public ImmutableArray<IResourceEntrySymbol> Resources { get; }

    // IEntrySymbol.ReferencedEntry
    IEntrySymbol? IEntrySymbol.ReferencedEntry => null;

    // ISymbol
    public ISymbol OriginalDefinition => OriginalEntry;
    public SymbolKind Kind => OriginalEntry.Kind;
    public string? Id => OriginalEntry.Id;
    public string? Comment => OriginalEntry.Comment;
    public ISymbol? ContainingSymbol => OriginalEntry.ContainingSymbol;
    public IModuleSymbol? ContainingModule => OriginalEntry.ContainingModule;
    public IGamesystemNamespaceSymbol? ContainingNamespace => OriginalEntry.ContainingNamespace;

    public void Accept(SymbolVisitor visitor) => visitor.VisitContainerEntry(this);
    public TResult Accept<TResult>(SymbolVisitor<TResult> visitor) => visitor.VisitContainerEntry(this);
    public TResult Accept<TArgument, TResult>(SymbolVisitor<TArgument, TResult> visitor, TArgument argument) => visitor.VisitContainerEntry(this, argument);

    private static ImmutableArray<ICostSymbol> ExtractCosts(
        ImmutableArray<IResourceEntrySymbol> resources)
    {
        var builder = ImmutableArray.CreateBuilder<ICostSymbol>();
        foreach (var r in resources)
        {
            if (r is ICostSymbol cost)
                builder.Add(cost);
        }
        return builder.ToImmutable();
    }

    private static ImmutableArray<IConstraintSymbol> BuildEffectiveConstraints(
        ImmutableArray<IConstraintSymbol> originalConstraints,
        IReadOnlyDictionary<string, decimal> effectiveValues)
    {
        if (effectiveValues.Count == 0)
        {
            return originalConstraints;
        }
        var builder = ImmutableArray.CreateBuilder<IConstraintSymbol>(originalConstraints.Length);
        foreach (var constraint in originalConstraints)
        {
            if (constraint.Id is { } id && effectiveValues.TryGetValue(id, out var effectiveValue))
            {
                builder.Add(new EffectiveConstraintSymbol(constraint, effectiveValue));
            }
            else
            {
                builder.Add(constraint);
            }
        }
        return builder.MoveToImmutable();
    }

}
