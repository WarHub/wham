using WarHub.ArmouryModel.Source;

namespace WarHub.ArmouryModel.Concrete;

/// <summary>
/// Separate symbol that is essentially a child of <see cref="EntrySymbol"/>.
/// Always created for entries with an <see cref="IPublicationReferencingNode"/> declaration,
/// even when <see cref="PublicationId"/> is null.
/// </summary>
[GenerateSymbol(SymbolKind.PublicationReference)]
internal sealed partial class PublicationReferenceSymbol : SourceDeclaredSymbol, IPublicationReferenceSymbol
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

    public override string? Id => null;

    public override string Name => string.Empty;

    public override string? Comment => null;

    public string? PublicationId => PublicationRefDeclaration.PublicationId;

    [Bound]
    public IPublicationSymbol? Publication
    {
        get
        {
            if (PublicationRefDeclaration.PublicationId is not null)
                return GetBoundField(ref lazyPublication, PublicationRefDeclaration, static (b, d, decl) => b.BindPublicationSymbol(decl, d));
            return null;
        }
    }

    public string? Page => PublicationRefDeclaration.Page;

}
