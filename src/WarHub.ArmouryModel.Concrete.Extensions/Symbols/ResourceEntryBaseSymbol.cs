using WarHub.ArmouryModel.Source;

namespace WarHub.ArmouryModel.Concrete;

[GenerateSymbol(SymbolKind.ResourceEntry)]
internal abstract partial class ResourceEntryBaseSymbol : EntrySymbol, IResourceEntrySymbol
{
    protected ResourceEntryBaseSymbol(
        ISymbol containingSymbol,
        SourceNode declaration,
        DiagnosticBag diagnostics)
        : base(containingSymbol, declaration, diagnostics)
    {
    }

    public abstract ResourceKind ResourceKind { get; }

    public virtual IResourceDefinitionSymbol? Type => null;

    public override IResourceEntrySymbol? ReferencedEntry => null;

    public override ImmutableArray<ResourceEntryBaseSymbol> Resources =>
        ImmutableArray<ResourceEntryBaseSymbol>.Empty;
}
