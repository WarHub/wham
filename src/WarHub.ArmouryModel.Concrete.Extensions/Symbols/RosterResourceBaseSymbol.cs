using WarHub.ArmouryModel.Source;

namespace WarHub.ArmouryModel.Concrete;

[GenerateSymbol(SymbolKind.Resource)]
internal abstract partial class RosterResourceBaseSymbol : EntryInstanceSymbol, IResourceSymbol
{
    protected RosterResourceBaseSymbol(
        ISymbol? containingSymbol,
        SourceNode declaration,
        DiagnosticBag diagnostics)
        : base(containingSymbol, declaration, diagnostics)
    {
    }

    public abstract ResourceKind ResourceKind { get; }

    IResourceEntrySymbol IResourceSymbol.SourceEntry => (IResourceEntrySymbol)SourceEntry;

    public override ImmutableArray<RosterResourceBaseSymbol> Resources =>
        ImmutableArray<RosterResourceBaseSymbol>.Empty;
}
