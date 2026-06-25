namespace WarHub.ArmouryModel.Concrete;

/// <summary>
/// Wraps an <see cref="ICostSymbol"/> with an effective <see cref="Value"/>
/// computed by applying modifiers in a roster context.
/// Delegates all other properties to the <see cref="OriginalCost"/>.
/// </summary>
[GenerateSymbol(SymbolKind.ResourceEntry)]
internal sealed partial class EffectiveCostSymbol : ICostSymbol
{
    public EffectiveCostSymbol(ICostSymbol original, decimal effectiveValue)
    {
        OriginalCost = original;
        Value = effectiveValue;
    }

    public ICostSymbol OriginalCost { get; }

    // Overridden
    public decimal Value { get; }

    // Delegated from ICostSymbol : IResourceEntrySymbol
    public ResourceKind ResourceKind => OriginalCost.ResourceKind;
    public IResourceDefinitionSymbol? Type => OriginalCost.Type;

    // Standalone effective values (not delegated — effective resources are never links)
    public bool IsHidden => OriginalCost.IsHidden;
    public bool IsReference => false;
    public IPublicationReferenceSymbol? PublicationReference => OriginalCost.PublicationReference;
    public ImmutableArray<IEffectSymbol> Effects => ImmutableArray<IEffectSymbol>.Empty;
    public ImmutableArray<IResourceEntrySymbol> Resources => ImmutableArray<IResourceEntrySymbol>.Empty;

    // IResourceEntrySymbol.ReferencedEntry (explicit for `new` member)
    IResourceEntrySymbol? IResourceEntrySymbol.ReferencedEntry => null;

    // IEntrySymbol.ReferencedEntry
    IEntrySymbol? IEntrySymbol.ReferencedEntry => null;

    // ISymbol
    public ISymbol OriginalDefinition => OriginalCost;
    public SymbolKind Kind => OriginalCost.Kind;
    public string? Id => OriginalCost.Id;
    public string Name => OriginalCost.Name;
    public string? Comment => OriginalCost.Comment;
    public ISymbol? ContainingSymbol => OriginalCost.ContainingSymbol;
    public IModuleSymbol? ContainingModule => OriginalCost.ContainingModule;
    public IGamesystemNamespaceSymbol? ContainingNamespace => OriginalCost.ContainingNamespace;
}
