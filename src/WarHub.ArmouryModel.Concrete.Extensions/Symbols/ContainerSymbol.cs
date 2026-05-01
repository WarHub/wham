using WarHub.ArmouryModel.Source;

namespace WarHub.ArmouryModel.Concrete;

[GenerateSymbol(SymbolKind.Container)]
internal abstract partial class ContainerSymbol : EntryInstanceSymbol, IContainerSymbol
{
    protected ContainerSymbol(
        ISymbol? containingSymbol,
        RosterElementBaseNode declaration,
        DiagnosticBag diagnostics)
        : base(containingSymbol, declaration, diagnostics)
    {
        Declaration = declaration;
        Resources = CreateRosterEntryResources().ToImmutableArray();

        IEnumerable<RosterResourceBaseSymbol> CreateRosterEntryResources()
        {
            foreach (var item in declaration.Rules)
            {
                yield return new RosterRuleSymbol(this, item, diagnostics);
            }
            foreach (var item in declaration.Profiles)
            {
                yield return new RosterProfileSymbol(this, item, diagnostics);
            }
        }
    }

    public new RosterElementBaseNode Declaration { get; }

    /// <summary>
    /// Walks the <see cref="Symbol.ContainingSymbol"/> chain to find the
    /// containing <see cref="RosterSymbol"/>. Returns <c>null</c> if not
    /// contained within a roster (shouldn't happen in normal compilation).
    /// </summary>
    internal RosterSymbol? GetRosterSymbol()
    {
        for (ISymbol? sym = ContainingSymbol; sym is not null; sym = sym.ContainingSymbol)
        {
            if (sym is RosterSymbol roster)
                return roster;
        }
        return null;
    }

    public abstract ContainerKind ContainerKind { get; }

    public string? CustomName => Declaration.CustomName;

    public string? CustomNotes => Declaration.CustomNotes;

    public override ImmutableArray<RosterResourceBaseSymbol> Resources { get; }

}
