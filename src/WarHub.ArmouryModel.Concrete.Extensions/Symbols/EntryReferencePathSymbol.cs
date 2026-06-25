using WarHub.ArmouryModel.Source;

namespace WarHub.ArmouryModel.Concrete;

[GenerateSymbol(SymbolKind.EntryReferencePath)]
internal abstract partial class EntryReferencePathBaseSymbol : SourceDeclaredSymbol, IEntryReferencePathSymbol
{
    private ImmutableArray<IEntrySymbol> lazySourceEntries;

    public EntryReferencePathBaseSymbol(ISymbol? containingSymbol, SourceNode declaration)
        : base(containingSymbol, declaration)
    {
    }

    public static EntryReferencePathBaseSymbol Create(ISymbol? containingSymbol, SourceNode declaration) => declaration switch
    {
        SelectionNode node => new SelectionEntryReferencePathSymbol(containingSymbol, node),
        ProfileNode node => new ProfileEntryReferencePathSymbol(containingSymbol, node),
        RuleNode node => new RuleEntryReferencePathSymbol(containingSymbol, node),
        _ when containingSymbol is EntryInstanceSymbol rosterEntry => new SingleEntryReferencePathSymbol(rosterEntry, declaration),
        _ => throw new InvalidOperationException("Unknown reference path declaration.")
    };

    [Bound]
    public ImmutableArray<IEntrySymbol> SourceEntries =>
        GetBoundField(ref lazySourceEntries, this, static (b, d, self) => self.BindSourceEntries(b, d));

    protected abstract ImmutableArray<IEntrySymbol> BindSourceEntries(Binder binder, BindingDiagnosticBag diagnostics);

    internal sealed class SingleEntryReferencePathSymbol : EntryReferencePathBaseSymbol
    {
        public SingleEntryReferencePathSymbol(EntryInstanceSymbol containingSymbol, SourceNode declaration)
            : base(containingSymbol, declaration)
        {
            ContainingSymbol = containingSymbol;
        }

        public new EntryInstanceSymbol ContainingSymbol { get; }

        protected override ImmutableArray<IEntrySymbol> BindSourceEntries(Binder binder, BindingDiagnosticBag diagnostics) =>
            ImmutableArray.Create(ContainingSymbol.SourceEntry);
    }

    internal sealed class SelectionEntryReferencePathSymbol : EntryReferencePathBaseSymbol
    {
        public SelectionEntryReferencePathSymbol(
            ISymbol? containingSymbol,
            SelectionNode declaration)
            : base(containingSymbol, declaration)
        {
            Declaration = declaration;
        }

        public override SelectionNode Declaration { get; }

        protected override ImmutableArray<IEntrySymbol> BindSourceEntries(Binder binder, BindingDiagnosticBag diagnostics) =>
            binder.BindSelectionSourcePathSymbol(Declaration, diagnostics);
    }

    internal sealed class ProfileEntryReferencePathSymbol : EntryReferencePathBaseSymbol
    {
        public ProfileEntryReferencePathSymbol(
            ISymbol? containingSymbol,
            ProfileNode declaration)
            : base(containingSymbol, declaration)
        {
            Declaration = declaration;
        }

        public override ProfileNode Declaration { get; }

        protected override ImmutableArray<IEntrySymbol> BindSourceEntries(Binder binder, BindingDiagnosticBag diagnostics) =>
            binder.BindProfileSourcePathSymbol(Declaration, diagnostics);
    }

    internal sealed class RuleEntryReferencePathSymbol : EntryReferencePathBaseSymbol
    {
        public RuleEntryReferencePathSymbol(
            ISymbol? containingSymbol,
            RuleNode declaration)
            : base(containingSymbol, declaration)
        {
            Declaration = declaration;
        }

        public override RuleNode Declaration { get; }

        protected override ImmutableArray<IEntrySymbol> BindSourceEntries(Binder binder, BindingDiagnosticBag diagnostics) =>
            binder.BindRuleSourcePathSymbol(Declaration, diagnostics);
    }
}
