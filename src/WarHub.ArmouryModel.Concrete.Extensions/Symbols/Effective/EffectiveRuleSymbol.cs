namespace WarHub.ArmouryModel.Concrete;

/// <summary>
/// Wraps an <see cref="IRuleSymbol"/> with effective (modifier-applied) values
/// computed in a roster context. Overrides <see cref="Name"/>, <see cref="IsHidden"/>,
/// and <see cref="DescriptionText"/>. Delegates all other properties to the
/// <see cref="OriginalRule"/>.
/// </summary>
internal sealed class EffectiveRuleSymbol : IRuleSymbol
{
    public EffectiveRuleSymbol(
        IRuleSymbol original,
        string effectiveName,
        bool effectiveIsHidden,
        string effectiveDescription)
    {
        OriginalRule = original;
        Name = effectiveName;
        IsHidden = effectiveIsHidden;
        DescriptionText = effectiveDescription;
    }

    public IRuleSymbol OriginalRule { get; }

    // Overridden
    public string Name { get; }
    public bool IsHidden { get; }
    public string DescriptionText { get; }

    // Delegated from IRuleSymbol : IResourceEntrySymbol
    public ResourceKind ResourceKind => OriginalRule.ResourceKind;
    public IResourceDefinitionSymbol? Type => OriginalRule.Type;

    // Delegated from IEntrySymbol
    public bool IsReference => OriginalRule.IsReference;
    public IPublicationReferenceSymbol? PublicationReference => OriginalRule.PublicationReference;
    public ImmutableArray<IEffectSymbol> Effects => OriginalRule.Effects;
    public ImmutableArray<IResourceEntrySymbol> Resources => OriginalRule.Resources;

    // IResourceEntrySymbol.ReferencedEntry (explicit for `new` member)
    IResourceEntrySymbol? IResourceEntrySymbol.ReferencedEntry => OriginalRule.ReferencedEntry;

    // IEntrySymbol.ReferencedEntry
    IEntrySymbol? IEntrySymbol.ReferencedEntry => ((IEntrySymbol)OriginalRule).ReferencedEntry;

    // Delegated from ISymbol
    public SymbolKind Kind => OriginalRule.Kind;
    public string? Id => OriginalRule.Id;
    public string? Comment => OriginalRule.Comment;
    public ISymbol? ContainingSymbol => OriginalRule.ContainingSymbol;
    public IModuleSymbol? ContainingModule => OriginalRule.ContainingModule;
    public IGamesystemNamespaceSymbol? ContainingNamespace => OriginalRule.ContainingNamespace;

    public void Accept(SymbolVisitor visitor) => visitor.VisitResourceEntry(this);
    public TResult Accept<TResult>(SymbolVisitor<TResult> visitor) => visitor.VisitResourceEntry(this);
    public TResult Accept<TArgument, TResult>(SymbolVisitor<TArgument, TResult> visitor, TArgument argument) => visitor.VisitResourceEntry(this, argument);
}
