namespace WarHub.ArmouryModel;

/// <summary>
/// Defines cost of an entry.
/// BS Cost on SelectionEntries.
/// WHAM <see cref="Source.CostNode" />.
/// </summary>
public interface ICostSymbol : IResourceEntrySymbol
{
    /// <summary>
    /// The raw cost type identifier string from the source data.
    /// Non-lazy alternative to <see cref="IResourceEntrySymbol.Type"/>.<see cref="ISymbol.Id"/>.
    /// </summary>
    string? TypeId { get; }

    decimal Value { get; }
}
