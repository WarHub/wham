namespace WarHub.ArmouryModel.Concrete;

/// <summary>
/// Abstract base for effective container entry symbols (selection entries and force entries).
/// Mirrors the <see cref="ContainerEntryBaseSymbol"/> hierarchy for non-effective symbols.
/// <para>
/// Overridden members: <see cref="Name"/>, <see cref="IsHidden"/>,
/// <see cref="Constraints"/>, <see cref="Costs"/>, <see cref="Resources"/>,
/// <see cref="PublicationReference"/>.
/// </para>
/// Standalone: <see cref="IsReference"/>=false, <see cref="Effects"/>=empty.
/// All other members delegate to <see cref="OriginalContainerEntry"/>.
/// </summary>
[GenerateSymbol(SymbolKind.ContainerEntry)]
internal abstract partial class EffectiveContainerEntrySymbol : IContainerEntrySymbol
{
    protected EffectiveContainerEntrySymbol(
        IContainerEntrySymbol original,
        string effectiveName,
        bool effectiveIsHidden,
        IReadOnlyDictionary<string, decimal> effectiveConstraintValues,
        ImmutableArray<IResourceEntrySymbol> effectiveResources,
        IPublicationReferenceSymbol? effectivePublicationReference)
    {
        Name = effectiveName;
        IsHidden = effectiveIsHidden;
        Constraints = BuildEffectiveConstraints(original.Constraints, effectiveConstraintValues);
        Resources = effectiveResources;
        Costs = ExtractCosts(effectiveResources, original.Costs.Length);
        PublicationReference = effectivePublicationReference;
    }

    /// <summary>
    /// The declared (unmodified) container entry this effective symbol wraps.
    /// Subclasses provide a typed accessor for their specific original.
    /// </summary>
    protected abstract IContainerEntrySymbol OriginalContainerEntry { get; }

    // Overridden effective values
    public string Name { get; }
    public bool IsHidden { get; }
    public ImmutableArray<IConstraintSymbol> Constraints { get; }
    public ImmutableArray<ICostSymbol> Costs { get; }
    public ImmutableArray<IResourceEntrySymbol> Resources { get; }
    public IPublicationReferenceSymbol? PublicationReference { get; }

    // Delegated from IContainerEntrySymbol
    public ContainerKind ContainerKind => OriginalContainerEntry.ContainerKind;

    // Standalone effective values (not delegated — effective entry is never a link)
    public bool IsReference => false;
    public ImmutableArray<IEffectSymbol> Effects => ImmutableArray<IEffectSymbol>.Empty;

    // IEntrySymbol.ReferencedEntry
    IEntrySymbol? IEntrySymbol.ReferencedEntry => null;

    // ISymbol — delegated to original
    public ISymbol OriginalDefinition => OriginalContainerEntry;
    public SymbolKind Kind => OriginalContainerEntry.Kind;
    public string? Id => OriginalContainerEntry.Id;
    public string? Comment => OriginalContainerEntry.Comment;
    public ISymbol? ContainingSymbol => OriginalContainerEntry.ContainingSymbol;
    public IModuleSymbol? ContainingModule => OriginalContainerEntry.ContainingModule;
    public IGamesystemNamespaceSymbol? ContainingNamespace => OriginalContainerEntry.ContainingNamespace;

    protected static ImmutableArray<ICostSymbol> ExtractCosts(
        ImmutableArray<IResourceEntrySymbol> resources, int capacityHint)
    {
        var builder = ImmutableArray.CreateBuilder<ICostSymbol>(capacityHint);
        foreach (var r in resources)
        {
            if (r is ICostSymbol cost)
                builder.Add(cost);
        }
        return builder.DrainToImmutable();
    }

    protected static ImmutableArray<IConstraintSymbol> BuildEffectiveConstraints(
        ImmutableArray<IConstraintSymbol> originalConstraints,
        IReadOnlyDictionary<string, decimal> effectiveValues)
    {
        if (effectiveValues.Count == 0)
        {
            return originalConstraints;
        }
        var anyChanged = false;
        var builder = ImmutableArray.CreateBuilder<IConstraintSymbol>(originalConstraints.Length);
        foreach (var constraint in originalConstraints)
        {
            if (constraint.Id is { } id
                && effectiveValues.TryGetValue(id, out var effectiveValue)
                && effectiveValue != constraint.Query.ReferenceValue)
            {
                anyChanged = true;
                builder.Add(new EffectiveConstraintSymbol(constraint, effectiveValue));
            }
            else
            {
                builder.Add(constraint);
            }
        }
        return anyChanged ? builder.MoveToImmutable() : originalConstraints;
    }
}
