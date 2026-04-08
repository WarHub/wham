namespace WarHub.ArmouryModel.Concrete;

/// <summary>
/// Wraps an <see cref="ICharacteristicSymbol"/> with an effective <see cref="Value"/>
/// computed by applying modifiers in a roster context.
/// Delegates all other properties to the <see cref="OriginalCharacteristic"/>.
/// </summary>
internal sealed class EffectiveCharacteristicSymbol : ICharacteristicSymbol
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

    // Delegated from IEntrySymbol
    public bool IsHidden => OriginalCharacteristic.IsHidden;
    public bool IsReference => OriginalCharacteristic.IsReference;
    public IPublicationReferenceSymbol? PublicationReference => OriginalCharacteristic.PublicationReference;
    public ImmutableArray<IEffectSymbol> Effects => OriginalCharacteristic.Effects;
    public ImmutableArray<IResourceEntrySymbol> Resources => OriginalCharacteristic.Resources;

    // IResourceEntrySymbol.ReferencedEntry (explicit for `new` member)
    IResourceEntrySymbol? IResourceEntrySymbol.ReferencedEntry => OriginalCharacteristic.ReferencedEntry;

    // IEntrySymbol.ReferencedEntry
    IEntrySymbol? IEntrySymbol.ReferencedEntry => ((IEntrySymbol)OriginalCharacteristic).ReferencedEntry;

    // Delegated from ISymbol
    public SymbolKind Kind => OriginalCharacteristic.Kind;
    public string? Id => OriginalCharacteristic.Id;
    public string Name => OriginalCharacteristic.Name;
    public string? Comment => OriginalCharacteristic.Comment;
    public ISymbol? ContainingSymbol => OriginalCharacteristic.ContainingSymbol;
    public IModuleSymbol? ContainingModule => OriginalCharacteristic.ContainingModule;
    public IGamesystemNamespaceSymbol? ContainingNamespace => OriginalCharacteristic.ContainingNamespace;

    public void Accept(SymbolVisitor visitor) => visitor.VisitResourceEntry(this);
    public TResult Accept<TResult>(SymbolVisitor<TResult> visitor) => visitor.VisitResourceEntry(this);
    public TResult Accept<TArgument, TResult>(SymbolVisitor<TArgument, TResult> visitor, TArgument argument) => visitor.VisitResourceEntry(this, argument);
}
