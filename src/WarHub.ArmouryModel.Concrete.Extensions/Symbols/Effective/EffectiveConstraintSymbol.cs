namespace WarHub.ArmouryModel.Concrete;

/// <summary>
/// Wraps an <see cref="IConstraintSymbol"/> with an effective query whose
/// <see cref="IQuerySymbol.ReferenceValue"/> has been computed by applying modifiers.
/// Delegates all other properties to the <see cref="OriginalConstraint"/>.
/// </summary>
internal sealed class EffectiveConstraintSymbol : IConstraintSymbol
{
    public EffectiveConstraintSymbol(IConstraintSymbol original, decimal? effectiveReferenceValue)
    {
        OriginalConstraint = original;
        Query = new EffectiveQuerySymbol(original.Query, effectiveReferenceValue);
    }

    public IConstraintSymbol OriginalConstraint { get; }

    // Overridden
    public IQuerySymbol Query { get; }

    // Delegated
    public SymbolKind Kind => OriginalConstraint.Kind;
    public string? Id => OriginalConstraint.Id;
    public string Name => OriginalConstraint.Name;
    public string? Comment => OriginalConstraint.Comment;
    public ISymbol? ContainingSymbol => OriginalConstraint.ContainingSymbol;
    public IModuleSymbol? ContainingModule => OriginalConstraint.ContainingModule;
    public IGamesystemNamespaceSymbol? ContainingNamespace => OriginalConstraint.ContainingNamespace;

    public void Accept(SymbolVisitor visitor) => visitor.VisitConstraint(this);
    public TResult Accept<TResult>(SymbolVisitor<TResult> visitor) => visitor.VisitConstraint(this);
    public TResult Accept<TArgument, TResult>(SymbolVisitor<TArgument, TResult> visitor, TArgument argument) => visitor.VisitConstraint(this, argument);
}
