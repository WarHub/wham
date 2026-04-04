using WarHub.ArmouryModel.Source;

namespace WarHub.ArmouryModel.Concrete;

internal sealed class ForceSymbol : ContainerSymbol, IForceSymbol, INodeDeclaredSymbol<ForceNode>
{
    private IForceEntrySymbol? lazyForceEntry;
    private ImmutableArray<IEffectiveProfileSymbol> lazyEffectiveProfiles;
    private ImmutableArray<IEffectiveRuleSymbol> lazyEffectiveRules;

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

    public override IForceEntrySymbol SourceEntry => GetBoundField(ref lazyForceEntry);

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

    ImmutableArray<IEffectiveProfileSymbol> IForceSymbol.EffectiveProfiles
    {
        get
        {
            EnsureEffectiveResources();
            return lazyEffectiveProfiles;
        }
    }

    ImmutableArray<IEffectiveRuleSymbol> IForceSymbol.EffectiveRules
    {
        get
        {
            EnsureEffectiveResources();
            return lazyEffectiveRules;
        }
    }

    private void EnsureEffectiveResources()
    {
        if (!lazyEffectiveProfiles.IsDefault)
            return;

        var roster = GetRosterSymbol();
        if (roster is not null)
        {
            // Force-level resources are resolved with null selection/force context
            // matching BattleScribe behavior (force entry modifiers don't have selection context).
            var cache = roster.GetOrCreateEffectiveEntryCache();
            var (profiles, rules) = cache.CollectEffectiveResources(SourceEntry, selection: null, force: null);
            lazyEffectiveProfiles = profiles;
            lazyEffectiveRules = rules;
        }
        else
        {
            lazyEffectiveProfiles = ImmutableArray<IEffectiveProfileSymbol>.Empty;
            lazyEffectiveRules = ImmutableArray<IEffectiveRuleSymbol>.Empty;
        }
    }

    protected override void BindReferencesCore(Binder binder, BindingDiagnosticBag diagnostics)
    {
        base.BindReferencesCore(binder, diagnostics);
        lazyForceEntry = binder.BindForceEntrySymbol(Declaration, diagnostics);
    }

    protected override ImmutableArray<Symbol> MakeAllMembers(BindingDiagnosticBag diagnostics) =>
        base.MakeAllMembers(diagnostics)
        .Add(CatalogueReference)
        .AddRange(Categories)
        .AddRange(Publications)
        .AddRange(Forces)
        .AddRange(ChildSelections);
}
