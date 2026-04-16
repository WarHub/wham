using WarHub.ArmouryModel.Source;

namespace WarHub.ArmouryModel.Concrete;

internal sealed class RosterProfileSymbol : RosterResourceBaseSymbol, IRosterProfileSymbol, INodeDeclaredSymbol<ProfileNode>
{
    private IResourceDefinitionSymbol? lazyType;

    public RosterProfileSymbol(ISymbol? containingSymbol, ProfileNode declaration, DiagnosticBag diagnostics) : base(containingSymbol, declaration, diagnostics)
    {
        Declaration = declaration;
        Characteristics = declaration.Characteristics.Select(x => new CharacteristicSymbol(this, x, diagnostics)).ToImmutableArray();
    }

    public new ProfileNode Declaration { get; }

    public override ResourceKind ResourceKind => ResourceKind.Profile;

    public IResourceDefinitionSymbol Type =>
        GetBoundField(ref lazyType, (b, d) => b.BindProfileTypeSymbol(Declaration, d));

    protected override void CheckReferencesCore() => _ = Type;

    public ImmutableArray<CharacteristicSymbol> Characteristics { get; }

    public override IProfileSymbol SourceEntry => (IProfileSymbol)SourceEntryPath.SourceEntries.Last();

    ImmutableArray<ICharacteristicSymbol> IRosterProfileSymbol.Characteristics =>
        Characteristics.Cast<CharacteristicSymbol, ICharacteristicSymbol>();

    protected override ImmutableArray<Symbol> MakeAllMembers(BindingDiagnosticBag diagnostics) =>
        base.MakeAllMembers(diagnostics)
        .AddRange(Characteristics.Cast<CharacteristicSymbol, Symbol>());
}
