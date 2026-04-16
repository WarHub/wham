using WarHub.ArmouryModel.Source;

namespace WarHub.ArmouryModel.Concrete;

/// <summary>
/// Separate symbol that is essentially a child of <see cref="EntrySymbol"/>.
/// Always created for entries with an <see cref="IPublicationReferencingNode"/> declaration,
/// even when <see cref="PublicationId"/> is null.
/// </summary>
internal sealed class PublicationReferenceSymbol : SourceDeclaredSymbol, IPublicationReferenceSymbol
{
    private IPublicationSymbol? lazyPublication;

    private PublicationReferenceSymbol(
        SourceDeclaredSymbol containingSymbol,
        IPublicationReferencingNode declaration)
        : base(containingSymbol, (SourceNode)declaration)
    {
        PublicationRefDeclaration = declaration;
    }

    public static PublicationReferenceSymbol? Create(
        SourceDeclaredSymbol containingSymbol,
        SourceNode declaration,
        DiagnosticBag diagnostics)
    {
        return declaration is IPublicationReferencingNode node
            ? new PublicationReferenceSymbol(containingSymbol, node)
            : null;
    }

    public IPublicationReferencingNode PublicationRefDeclaration { get; }

    public sealed override SymbolKind Kind => SymbolKind.Link;

    public override string? Id => null;

    public override string Name => string.Empty;

    public override string? Comment => null;

    public string? PublicationId => PublicationRefDeclaration.PublicationId;

    public IPublicationSymbol? Publication
    {
        get
        {
            if (PublicationRefDeclaration.PublicationId is not null)
                return GetBoundField(ref lazyPublication, (b, d) => b.BindPublicationSymbol(PublicationRefDeclaration, d));
            return null;
        }
    }

    public string? Page => PublicationRefDeclaration.Page;

    protected override void CheckReferencesCore() => _ = Publication;

    public sealed override void Accept(SymbolVisitor visitor) =>
        visitor.VisitPublicationReference(this);

    public sealed override TResult Accept<TResult>(SymbolVisitor<TResult> visitor) =>
        visitor.VisitPublicationReference(this);

    public sealed override TResult Accept<TArgument, TResult>(SymbolVisitor<TArgument, TResult> visitor, TArgument argument) =>
        visitor.VisitPublicationReference(this, argument);
}
