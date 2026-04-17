using WarHub.ArmouryModel.Source;

namespace WarHub.ArmouryModel.Concrete;

internal sealed class ForceSymbol : ContainerSymbol, IForceSymbol, INodeDeclaredSymbol<ForceNode>
{
    private IForceEntrySymbol? lazyForceEntry;
    private IForceEntrySymbol? lazyEffectiveSourceEntry;

    public ForceSymbol(
        ISymbol? containingSymbol,
        ForceNode declaration,
        DiagnosticBag diagnostics)
        : base(containingSymbol, declaration, diagnostics)
    {
        Declaration = declaration;
        CatalogueReference = new ForceCatalogueReferenceSymbol(this, declaration, diagnostics);
        Categories = declaration.Categories.Select(x => new CategorySymbol(this, x, diagnostics)).ToImmutableArray();
        Publications = declaration.Publications.Select(x => new PublicationSymbol(this, x, diagnostics)).ToImmutableArray();
        Forces = declaration.Forces.Select(x => new ForceSymbol(this, x, diagnostics)).ToImmutableArray();
        ChildSelections = declaration.Selections.Select(x => new SelectionSymbol(this, x, diagnostics)).ToImmutableArray();
    }

    public new ForceNode Declaration { get; }

    public override ContainerKind ContainerKind => ContainerKind.Force;

    public string? EntryId => Declaration.EntryId;

    public override IForceEntrySymbol SourceEntry =>
        GetBoundField(ref lazyForceEntry, Declaration, static (b, d, decl) => b.BindForceEntrySymbol(decl, d));

    protected override void CheckReferencesCore() => _ = SourceEntry;

    public ImmutableArray<ForceSymbol> Forces { get; }

    public ImmutableArray<SelectionSymbol> ChildSelections { get; }

    public ImmutableArray<CategorySymbol> Categories { get; }

    public ImmutableArray<PublicationSymbol> Publications { get; }

    public ForceCatalogueReferenceSymbol CatalogueReference { get; }

    ICatalogueReferenceSymbol IForceSymbol.CatalogueReference => CatalogueReference;

    ImmutableArray<IForceSymbol> IForceContainerSymbol.Forces =>
        Forces.Cast<ForceSymbol, IForceSymbol>();

    ImmutableArray<ICategorySymbol> IForceSymbol.Categories =>
        Categories.Cast<CategorySymbol, ICategorySymbol>();

    ImmutableArray<IPublicationSymbol> IForceSymbol.Publications =>
        Publications.Cast<PublicationSymbol, IPublicationSymbol>();

    ImmutableArray<ISelectionSymbol> ISelectionContainerSymbol.Selections =>
        ChildSelections.Cast<SelectionSymbol, ISelectionSymbol>();

    public ISelectionEntryContainerSymbol GetEffectiveEntry(
        ISelectionEntryContainerSymbol declaredEntry)
    {
        var roster = GetRosterSymbol();
        return roster?.GetOrCreateEffectiveEntryCache().GetEffectiveEntry(declaredEntry, selection: null, this)
            ?? declaredEntry;
    }

    IForceEntrySymbol IForceSymbol.EffectiveSourceEntry
    {
        get
        {
            if (lazyEffectiveSourceEntry is { } cached)
                return cached;
            var roster = GetRosterSymbol();
            IForceEntrySymbol result;
            if (roster is not null)
            {
                var cache = roster.GetOrCreateEffectiveEntryCache();
                result = cache.CreateEffectiveForceEntry(SourceEntry, this);
            }
            else
            {
                result = SourceEntry;
            }
            Interlocked.CompareExchange(ref lazyEffectiveSourceEntry, result, null);
            return lazyEffectiveSourceEntry;
        }
    }

    protected override ImmutableArray<Symbol> MakeAllMembers(BindingDiagnosticBag diagnostics) =>
        base.MakeAllMembers(diagnostics)
        .Add(CatalogueReference)
        .AddRange(Categories)
        .AddRange(Publications)
        .AddRange(Forces)
        .AddRange(ChildSelections);
}
