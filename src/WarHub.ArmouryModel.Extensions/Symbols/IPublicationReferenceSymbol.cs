namespace WarHub.ArmouryModel;

/// <summary>
/// A reference of a publication symbol with a page detail.
/// Always created for entries with an <see cref="Source.IPublicationReferencingNode"/> declaration,
/// even when <see cref="PublicationId"/> is null.
/// WHAM <see cref="Source.IPublicationReferencingNode" />.
/// </summary>
public interface IPublicationReferenceSymbol : ISymbol
{
    /// <summary>
    /// The raw declared publication ID, or null when the entry has no publication reference.
    /// </summary>
    string? PublicationId { get; }

    /// <summary>
    /// The resolved publication symbol. Null only when <see cref="PublicationId"/> is null.
    /// When binding fails, returns an error symbol.
    /// </summary>
    IPublicationSymbol? Publication { get; }

    /// <summary>
    /// Page reference from the declaration, or null when no page is specified.
    /// </summary>
    string? Page { get; }
}
