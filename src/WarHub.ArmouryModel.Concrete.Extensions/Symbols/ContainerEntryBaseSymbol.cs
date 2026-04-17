using WarHub.ArmouryModel.Source;

namespace WarHub.ArmouryModel.Concrete;

[GenerateSymbol(SymbolKind.ContainerEntry)]
internal abstract partial class ContainerEntryBaseSymbol : EntrySymbol, IContainerEntrySymbol
{
    protected ContainerEntryBaseSymbol(
        ISymbol containingSymbol,
        ContainerEntryBaseNode declaration,
        DiagnosticBag diagnostics)
        : base(containingSymbol, declaration, diagnostics)
    {
        Constraints = CreateConstraints().ToImmutableArray();
        Costs = CreateCosts().ToImmutableArray();
        Resources = CreateResourceEntries().ToImmutableArray();

        IEnumerable<ConstraintSymbol> CreateConstraints()
        {
            foreach (var item in declaration.Constraints)
            {
                yield return new ConstraintSymbol(this, item, diagnostics);
            }
        }

        IEnumerable<CostSymbol> CreateCosts()
        {
            var costs = declaration switch
            {
                SelectionEntryNode entry => entry.Costs.NodeList,
                EntryLinkNode link => link.Costs.NodeList,
                _ => default,
            };
            foreach (var item in costs)
            {
                yield return CreateEntry(this, item, diagnostics);
            }
        }

        IEnumerable<ResourceEntryBaseSymbol> CreateResourceEntries()
        {
            foreach (var item in Costs)
            {
                yield return item;
            }
            foreach (var item in declaration.InfoGroups)
            {
                yield return CreateEntry(this, item, diagnostics);
            }
            foreach (var item in declaration.InfoLinks)
            {
                yield return CreateEntry(this, item, diagnostics);
            }
            foreach (var item in declaration.Profiles)
            {
                yield return CreateEntry(this, item, diagnostics);
            }
            foreach (var item in declaration.Rules)
            {
                yield return CreateEntry(this, item, diagnostics);
            }
        }
    }

    public abstract ContainerKind ContainerKind { get; }

    public ImmutableArray<ConstraintSymbol> Constraints { get; }

    public sealed override ImmutableArray<ResourceEntryBaseSymbol> Resources { get; }

    public ImmutableArray<CostSymbol> Costs { get; }

    ImmutableArray<IConstraintSymbol> IContainerEntrySymbol.Constraints =>
        Constraints.Cast<ConstraintSymbol, IConstraintSymbol>();

    ImmutableArray<ICostSymbol> IContainerEntrySymbol.Costs =>
        Costs.Cast<CostSymbol, ICostSymbol>();

    protected override ImmutableArray<Symbol> MakeAllMembers(BindingDiagnosticBag diagnostics) =>
        base.MakeAllMembers(diagnostics)
        .AddRange(Constraints);
}
