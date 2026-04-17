namespace WarHub.ArmouryModel.Concrete;

/// <summary>
/// Wraps an <see cref="ICharacteristicSymbol"/> with an effective <see cref="Value"/>
/// computed by applying modifiers in a roster context.
/// Delegates all other properties to the <see cref="OriginalCharacteristic"/>.
/// </summary>
[GenerateSymbol(SymbolKind.ResourceEntry)]
internal sealed partial class EffectiveCharacteristicSymbol : ICharacteristicSymbol
{
    public EffectiveCharacteristicSymbol(ICharacteristicSymbol original, string effectiveValue)
    {
        OriginalCharacteristic = original;
        Value = effectiveValue;
    }

    public ICharacteristicSymbol OriginalCharacteristic { get; }

    // Overridden
    public string Value { get; }

    // Delegated from ICharacteristicSymbol : IResourceEntrySymbol
    public ResourceKind ResourceKind => OriginalCharacteristic.ResourceKind;
    public IResourceDefinitionSymbol? Type => OriginalCharacteristic.Type;

    // Standalone effective values (not delegated — effective resources are never links)
    public bool IsHidden => OriginalCharacteristic.IsHidden;
    public bool IsReference => false;
    public IPublicationReferenceSymbol? PublicationReference => OriginalCharacteristic.PublicationReference;
    public ImmutableArray<IEffectSymbol> Effects => ImmutableArray<IEffectSymbol>.Empty;
    public ImmutableArray<IResourceEntrySymbol> Resources => ImmutableArray<IResourceEntrySymbol>.Empty;

    // IResourceEntrySymbol.ReferencedEntry (explicit for `new` member)
    IResourceEntrySymbol? IResourceEntrySymbol.ReferencedEntry => null;

    // IEntrySymbol.ReferencedEntry
    IEntrySymbol? IEntrySymbol.ReferencedEntry => null;

    // ISymbol
    public ISymbol OriginalDefinition => OriginalCharacteristic;
    public SymbolKind Kind => OriginalCharacteristic.Kind;
    public string? Id => OriginalCharacteristic.Id;
    public string Name => OriginalCharacteristic.Name;
    public string? Comment => OriginalCharacteristic.Comment;
    public ISymbol? ContainingSymbol => OriginalCharacteristic.ContainingSymbol;
    public IModuleSymbol? ContainingModule => OriginalCharacteristic.ContainingModule;
    public IGamesystemNamespaceSymbol? ContainingNamespace => OriginalCharacteristic.ContainingNamespace;
}
