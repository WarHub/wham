namespace WarHub.ArmouryModel.Concrete;

/// <summary>
/// Wraps an <see cref="IQuerySymbol"/> with an effective <see cref="ReferenceValue"/>
/// computed by applying modifiers in a roster context.
/// Delegates all other properties to the <see cref="OriginalQuery"/>.
/// </summary>
[GenerateSymbol(SymbolKind.Query)]
internal sealed partial class EffectiveQuerySymbol : IQuerySymbol
{
    public EffectiveQuerySymbol(IQuerySymbol original, decimal? effectiveReferenceValue)
    {
        OriginalQuery = original;
        ReferenceValue = effectiveReferenceValue;
    }

    public IQuerySymbol OriginalQuery { get; }

    // Overridden
    public decimal? ReferenceValue { get; }

    // ISymbol
    public ISymbol OriginalDefinition => OriginalQuery;
    public SymbolKind Kind => OriginalQuery.Kind;
    public string? Id => OriginalQuery.Id;
    public string Name => OriginalQuery.Name;
    public string? Comment => OriginalQuery.Comment;
    public ISymbol? ContainingSymbol => OriginalQuery.ContainingSymbol;
    public IModuleSymbol? ContainingModule => OriginalQuery.ContainingModule;
    public IGamesystemNamespaceSymbol? ContainingNamespace => OriginalQuery.ContainingNamespace;
    public QueryComparisonType Comparison => OriginalQuery.Comparison;
    public QueryValueKind ValueKind => OriginalQuery.ValueKind;
    public ISymbol? ValueTypeSymbol => OriginalQuery.ValueTypeSymbol;
    public QueryScopeKind ScopeKind => OriginalQuery.ScopeKind;
    public ISymbol? ScopeSymbol => OriginalQuery.ScopeSymbol;
    public QueryFilterKind ValueFilterKind => OriginalQuery.ValueFilterKind;
    public ISymbol? FilterSymbol => OriginalQuery.FilterSymbol;
    public QueryOptions Options => OriginalQuery.Options;
}
