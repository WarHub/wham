namespace WarHub.ArmouryModel;

/// <summary>
/// Lightweight, serializable identifier for an <see cref="ISymbol"/> that can be
/// resolved across compilations built from compatible data.
/// Analogous to Roslyn's internal SymbolKey.
/// </summary>
public readonly record struct SymbolKey
{
    /// <summary>
    /// The kind of symbol this key identifies.
    /// </summary>
    public SymbolKind Kind { get; init; }

    /// <summary>
    /// The <see cref="ISymbol.Id"/> of the target symbol.
    /// </summary>
    public string? SymbolId { get; init; }

    /// <summary>
    /// The <see cref="ISymbol.Id"/> of the containing module
    /// (<see cref="ICatalogueSymbol"/> or <see cref="IRosterSymbol"/>).
    /// </summary>
    public string? ContainingModuleId { get; init; }

    /// <summary>
    /// The <see cref="ISymbol.Id"/> of the containing entry, used to
    /// disambiguate nested entries that might share an ID with a root entry.
    /// <see langword="null"/> for root-level entries and top-level roster containers.
    /// </summary>
    public string? ContainingEntryId { get; init; }

    /// <summary>
    /// Creates a <see cref="SymbolKey"/> from the given symbol's position in the symbol hierarchy.
    /// </summary>
    public static SymbolKey Create(ISymbol symbol)
    {
        return new SymbolKey
        {
            Kind = symbol.Kind,
            SymbolId = symbol.Id,
            ContainingModuleId = symbol.ContainingModule?.Id,
            ContainingEntryId = GetContainingEntryId(symbol),
        };
    }

    /// <summary>
    /// Resolves this key in the given <paramref name="compilation"/>, returning
    /// a <see cref="SymbolKeyResolution"/> describing the outcome.
    /// </summary>
    public SymbolKeyResolution Resolve(Compilation compilation) =>
        compilation.ResolveSymbolKey(this);

    private static string? GetContainingEntryId(ISymbol symbol)
    {
        // Walk up from the symbol's immediate parent to find a containing entry
        // that provides disambiguation context.
        for (var parent = symbol.ContainingSymbol; parent is not null; parent = parent.ContainingSymbol)
        {
            switch (parent.Kind)
            {
                // Catalogue/Roster module boundary — no containing entry.
                case SymbolKind.Catalogue:
                case SymbolKind.Roster:
                case SymbolKind.Namespace:
                    return null;

                // Immediate parent is an entry or container — use its ID.
                case SymbolKind.ContainerEntry:
                case SymbolKind.Container:
                    return parent.Id;
            }
        }
        return null;
    }
}
