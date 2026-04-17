using WarHub.ArmouryModel.Source;

namespace WarHub.ArmouryModel.Concrete;

[GenerateSymbol(SymbolKind.ResourceDefinition)]
internal abstract partial class ResourceDefinitionBaseSymbol : SourceDeclaredSymbol, IResourceDefinitionSymbol
{
    protected ResourceDefinitionBaseSymbol(ISymbol? containingSymbol, SourceNode declaration) : base(containingSymbol, declaration)
    {
    }

    public abstract ResourceKind ResourceKind { get; }

    ImmutableArray<IResourceDefinitionSymbol> IResourceDefinitionSymbol.Definitions =>
        ImmutableArray<IResourceDefinitionSymbol>.Empty;
}
