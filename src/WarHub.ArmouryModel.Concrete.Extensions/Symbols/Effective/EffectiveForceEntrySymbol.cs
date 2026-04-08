namespace WarHub.ArmouryModel.Concrete;

/// <summary>
/// Wraps an <see cref="IForceEntrySymbol"/> with effective (modifier-applied) resources
/// computed in a roster context. Overrides <see cref="Resources"/> to return
/// effective profiles and rules. Delegates all other properties to the
/// <see cref="OriginalForceEntry"/>.
/// </summary>
internal sealed class EffectiveForceEntrySymbol : IForceEntrySymbol
{
    public EffectiveForceEntrySymbol(
        IForceEntrySymbol original,
        ImmutableArray<IResourceEntrySymbol> effectiveResources)
    {
        OriginalForceEntry = original;
        Resources = effectiveResources;
    }

    public IForceEntrySymbol OriginalForceEntry { get; }

    // Overridden — effective resources (flattened, modifier-applied)
    public ImmutableArray<IResourceEntrySymbol> Resources { get; }

    // Delegated from IForceEntrySymbol
    public ImmutableArray<IForceEntrySymbol> ChildForces => OriginalForceEntry.ChildForces;
    public ImmutableArray<ICategoryEntrySymbol> Categories => OriginalForceEntry.Categories;

    // Delegated from IContainerEntrySymbol
    public ContainerKind ContainerKind => OriginalForceEntry.ContainerKind;
    public ImmutableArray<IConstraintSymbol> Constraints => OriginalForceEntry.Constraints;
    public ImmutableArray<ICostSymbol> Costs => OriginalForceEntry.Costs;

    // Standalone effective values (not delegated — effective entry is never a link)
    public bool IsHidden => OriginalForceEntry.IsHidden;
    public bool IsReference => false;
    public IPublicationReferenceSymbol? PublicationReference => OriginalForceEntry.PublicationReference;
    public ImmutableArray<IEffectSymbol> Effects => ImmutableArray<IEffectSymbol>.Empty;

    // IEntrySymbol.ReferencedEntry
    IEntrySymbol? IEntrySymbol.ReferencedEntry => null;

    // Delegated from ISymbol
    public ISymbol OriginalDefinition => OriginalForceEntry;
    public SymbolKind Kind => OriginalForceEntry.Kind;
    public string? Id => OriginalForceEntry.Id;
    public string Name => OriginalForceEntry.Name;
    public string? Comment => OriginalForceEntry.Comment;
    public ISymbol? ContainingSymbol => OriginalForceEntry.ContainingSymbol;
    public IModuleSymbol? ContainingModule => OriginalForceEntry.ContainingModule;
    public IGamesystemNamespaceSymbol? ContainingNamespace => OriginalForceEntry.ContainingNamespace;

    public void Accept(SymbolVisitor visitor) => visitor.VisitContainerEntry(this);
    public TResult Accept<TResult>(SymbolVisitor<TResult> visitor) => visitor.VisitContainerEntry(this);
    public TResult Accept<TArgument, TResult>(SymbolVisitor<TArgument, TResult> visitor, TArgument argument) => visitor.VisitContainerEntry(this, argument);
}
