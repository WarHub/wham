using System.Runtime.CompilerServices;

namespace WarHub.ArmouryModel;

/// <summary>
/// Describes the result of resolving a <see cref="SymbolKey"/> against a <see cref="Compilation"/>.
/// </summary>
public enum SymbolKeyResolutionKind
{
    /// <summary>
    /// Exactly one symbol was found.
    /// </summary>
    Resolved,

    /// <summary>
    /// No matching symbol was found in the compilation.
    /// </summary>
    Missing,

    /// <summary>
    /// Multiple matching symbols were found. See <see cref="SymbolKeyResolution.CandidateSymbols"/>.
    /// </summary>
    Ambiguous,
}

/// <summary>
/// The result of resolving a <see cref="SymbolKey"/> against a <see cref="Compilation"/>.
/// </summary>
public readonly struct SymbolKeyResolution : IEquatable<SymbolKeyResolution>
{
    private SymbolKeyResolution(ISymbol? symbol, ImmutableArray<ISymbol> candidateSymbols, SymbolKeyResolutionKind kind)
    {
        Symbol = symbol;
        CandidateSymbols = candidateSymbols;
        Kind = kind;
    }

    /// <summary>
    /// The resolved symbol, or <see langword="null"/> if resolution was not successful.
    /// </summary>
    public ISymbol? Symbol { get; }

    /// <summary>
    /// When <see cref="Kind"/> is <see cref="SymbolKeyResolutionKind.Ambiguous"/>,
    /// contains the candidate symbols that matched.
    /// </summary>
    public ImmutableArray<ISymbol> CandidateSymbols { get; }

    /// <summary>
    /// Describes the outcome of the resolution.
    /// </summary>
    public SymbolKeyResolutionKind Kind { get; }

    /// <summary>
    /// Creates a successful resolution with exactly one symbol.
    /// </summary>
    public static SymbolKeyResolution Resolved(ISymbol symbol) =>
        new(symbol, ImmutableArray<ISymbol>.Empty, SymbolKeyResolutionKind.Resolved);

    /// <summary>
    /// Creates a missing resolution (no matching symbol found).
    /// </summary>
    public static SymbolKeyResolution Missing() =>
        new(null, ImmutableArray<ISymbol>.Empty, SymbolKeyResolutionKind.Missing);

    /// <summary>
    /// Creates an ambiguous resolution with multiple candidates.
    /// </summary>
    public static SymbolKeyResolution Ambiguous(ImmutableArray<ISymbol> candidates) =>
        new(null, candidates, SymbolKeyResolutionKind.Ambiguous);

    public bool Equals(SymbolKeyResolution other) =>
        Kind == other.Kind
        && ReferenceEquals(Symbol, other.Symbol)
        && CandidateSymbols.SequenceEqual(other.CandidateSymbols);

    public override bool Equals(object? obj) => obj is SymbolKeyResolution other && Equals(other);

    public override int GetHashCode() => HashCode.Combine(Kind, RuntimeHelpers.GetHashCode(Symbol));

    public static bool operator ==(SymbolKeyResolution left, SymbolKeyResolution right) => left.Equals(right);

    public static bool operator !=(SymbolKeyResolution left, SymbolKeyResolution right) => !left.Equals(right);
}
