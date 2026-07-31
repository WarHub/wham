namespace WarHub.ArmouryModel.Concrete;

/// <summary>
/// Wraps an <see cref="IPublicationReferenceSymbol"/> with an effective <see cref="Page"/>
/// computed by applying modifiers in a roster context.
/// Delegates all other properties to the <see cref="OriginalReference"/>.
/// </summary>
[GenerateSymbol(SymbolKind.PublicationReference)]
internal sealed partial class EffectivePublicationReferenceSymbol : IPublicationReferenceSymbol
{
    public EffectivePublicationReferenceSymbol(IPublicationReferenceSymbol original, string? effectivePage)
    {
        OriginalReference = original;
        Page = effectivePage;
    }

    public IPublicationReferenceSymbol OriginalReference { get; }

    // Overridden
    public string? Page { get; }

    // Delegated
    public string? PublicationId => OriginalReference.PublicationId;
    public IPublicationSymbol? Publication => OriginalReference.Publication;

    // ISymbol
    public ISymbol OriginalDefinition => OriginalReference;
    public SymbolKind Kind => OriginalReference.Kind;
    public string? Id => OriginalReference.Id;
    public string Name => OriginalReference.Name;
    public string? Comment => OriginalReference.Comment;
    public ISymbol? ContainingSymbol => OriginalReference.ContainingSymbol;
    public IModuleSymbol? ContainingModule => OriginalReference.ContainingModule;
    public IGamesystemNamespaceSymbol? ContainingNamespace => OriginalReference.ContainingNamespace;
}
