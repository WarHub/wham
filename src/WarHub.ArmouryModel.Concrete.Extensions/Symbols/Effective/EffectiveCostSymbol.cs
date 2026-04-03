namespace WarHub.ArmouryModel.Concrete;

/// <summary>
/// Wraps an <see cref="ICostSymbol"/> with an effective <see cref="Value"/>
/// computed by applying modifiers in a roster context.
/// Delegates all other properties to the <see cref="OriginalCost"/>.
/// </summary>
internal sealed class EffectiveCostSymbol : ICostSymbol
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

    // Delegated from IEntrySymbol
    public bool IsHidden => OriginalCost.IsHidden;
    public bool IsReference => OriginalCost.IsReference;
    public IPublicationReferenceSymbol? PublicationReference => OriginalCost.PublicationReference;
    public ImmutableArray<IEffectSymbol> Effects => OriginalCost.Effects;
    public ImmutableArray<IResourceEntrySymbol> Resources => OriginalCost.Resources;

    // IResourceEntrySymbol.ReferencedEntry (explicit for `new` member)
    IResourceEntrySymbol? IResourceEntrySymbol.ReferencedEntry => OriginalCost.ReferencedEntry;

    // IEntrySymbol.ReferencedEntry
    IEntrySymbol? IEntrySymbol.ReferencedEntry => ((IEntrySymbol)OriginalCost).ReferencedEntry;

    // Delegated from ISymbol
    public SymbolKind Kind => OriginalCost.Kind;
    public string? Id => OriginalCost.Id;
    public string Name => OriginalCost.Name;
    public string? Comment => OriginalCost.Comment;
    public ISymbol? ContainingSymbol => OriginalCost.ContainingSymbol;
    public IModuleSymbol? ContainingModule => OriginalCost.ContainingModule;
    public IGamesystemNamespaceSymbol? ContainingNamespace => OriginalCost.ContainingNamespace;

    public void Accept(SymbolVisitor visitor) => visitor.VisitResourceEntry(this);
    public TResult Accept<TResult>(SymbolVisitor<TResult> visitor) => visitor.VisitResourceEntry(this);
    public TResult Accept<TArgument, TResult>(SymbolVisitor<TArgument, TResult> visitor, TArgument argument) => visitor.VisitResourceEntry(this, argument);
}
