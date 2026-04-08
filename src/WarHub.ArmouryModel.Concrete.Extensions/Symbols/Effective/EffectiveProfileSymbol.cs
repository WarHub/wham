namespace WarHub.ArmouryModel.Concrete;

/// <summary>
/// Wraps an <see cref="IProfileSymbol"/> with effective (modifier-applied) values
/// computed in a roster context. Overrides <see cref="Name"/>, <see cref="IsHidden"/>,
/// and <see cref="Characteristics"/>. Delegates all other properties to the
/// <see cref="OriginalProfile"/>.
/// </summary>
internal sealed class EffectiveProfileSymbol : IProfileSymbol
{
    public EffectiveProfileSymbol(
        IProfileSymbol original,
        string effectiveName,
        bool effectiveIsHidden,
        ImmutableArray<ICharacteristicSymbol> effectiveCharacteristics)
    {
        OriginalProfile = original;
        Name = effectiveName;
        IsHidden = effectiveIsHidden;
        Characteristics = effectiveCharacteristics;
    }

    public IProfileSymbol OriginalProfile { get; }

    // Overridden
    public string Name { get; }
    public bool IsHidden { get; }
    public ImmutableArray<ICharacteristicSymbol> Characteristics { get; }

    // Delegated from IProfileSymbol : IResourceEntrySymbol
    public ResourceKind ResourceKind => OriginalProfile.ResourceKind;
    public IResourceDefinitionSymbol? Type => OriginalProfile.Type;

    // Standalone effective values (not delegated — effective resources are never links)
    public bool IsReference => false;
    public IPublicationReferenceSymbol? PublicationReference => OriginalProfile.PublicationReference;
    public ImmutableArray<IEffectSymbol> Effects => ImmutableArray<IEffectSymbol>.Empty;
    public ImmutableArray<IResourceEntrySymbol> Resources => ImmutableArray<IResourceEntrySymbol>.Empty;

    // IResourceEntrySymbol.ReferencedEntry (explicit for `new` member)
    IResourceEntrySymbol? IResourceEntrySymbol.ReferencedEntry => null;

    // IEntrySymbol.ReferencedEntry
    IEntrySymbol? IEntrySymbol.ReferencedEntry => null;

    // ISymbol
    public ISymbol OriginalDefinition => OriginalProfile;
    public SymbolKind Kind => OriginalProfile.Kind;
    public string? Id => OriginalProfile.Id;
    public string? Comment => OriginalProfile.Comment;
    public ISymbol? ContainingSymbol => OriginalProfile.ContainingSymbol;
    public IModuleSymbol? ContainingModule => OriginalProfile.ContainingModule;
    public IGamesystemNamespaceSymbol? ContainingNamespace => OriginalProfile.ContainingNamespace;

    public void Accept(SymbolVisitor visitor) => visitor.VisitResourceEntry(this);
    public TResult Accept<TResult>(SymbolVisitor<TResult> visitor) => visitor.VisitResourceEntry(this);
    public TResult Accept<TArgument, TResult>(SymbolVisitor<TArgument, TResult> visitor, TArgument argument) => visitor.VisitResourceEntry(this, argument);
}
