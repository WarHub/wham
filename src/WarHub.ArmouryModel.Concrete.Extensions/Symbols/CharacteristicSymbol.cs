using WarHub.ArmouryModel.Source;

namespace WarHub.ArmouryModel.Concrete;

internal sealed partial class CharacteristicSymbol : ResourceEntryBaseSymbol, ICharacteristicSymbol, INodeDeclaredSymbol<CharacteristicNode>
{
    private IResourceDefinitionSymbol? lazyType;

    public CharacteristicSymbol(
        ISymbol containingSymbol,
        CharacteristicNode declaration,
        DiagnosticBag diagnostics)
        : base(containingSymbol, declaration, diagnostics)
    {
        Declaration = declaration;
    }

    public override CharacteristicNode Declaration { get; }

    public override ResourceKind ResourceKind => ResourceKind.Characteristic;

    [Bound]
    public override IResourceDefinitionSymbol Type =>
        GetBoundField(ref lazyType, Declaration, static (b, d, decl) => b.BindCharacteristicTypeSymbol(decl, d));

    public string Value => Declaration.Value ?? string.Empty;
}
