using WarHub.ArmouryModel.Source;

namespace WarHub.ArmouryModel.Concrete;

internal sealed class ProfileSymbol : ResourceEntryBaseSymbol, IProfileSymbol, INodeDeclaredSymbol<ProfileNode>
{
    private IResourceDefinitionSymbol? lazyType;

    public ProfileSymbol(
        ISymbol containingSymbol,
        ProfileNode declaration,
        DiagnosticBag diagnostics)
        : base(containingSymbol, declaration, diagnostics)
    {
        Declaration = declaration;
        Characteristics = CreateCharacteristics().ToImmutableArray();

        IEnumerable<CharacteristicSymbol> CreateCharacteristics()
        {
            foreach (var item in declaration.Characteristics)
            {
                yield return new CharacteristicSymbol(this, item, diagnostics);
            }
        }
    }

    public override ProfileNode Declaration { get; }

    public override IResourceDefinitionSymbol Type =>
        GetBoundField(ref lazyType, (b, d) => b.BindProfileTypeSymbol(Declaration, d));

    protected override void CheckReferencesCore() => _ = Type;

    public override ResourceKind ResourceKind => ResourceKind.Profile;

    public ImmutableArray<CharacteristicSymbol> Characteristics { get; }

    public override ImmutableArray<ResourceEntryBaseSymbol> Resources =>
        Characteristics.Cast<CharacteristicSymbol, ResourceEntryBaseSymbol>();

    ImmutableArray<IResourceEntrySymbol> IEntrySymbol.Resources =>
        Characteristics.Cast<CharacteristicSymbol, IResourceEntrySymbol>();

    ImmutableArray<ICharacteristicSymbol> IProfileSymbol.Characteristics =>
        Characteristics.Cast<CharacteristicSymbol, ICharacteristicSymbol>();
}
