using WarHub.ArmouryModel.Source;

namespace WarHub.ArmouryModel.Concrete;

internal sealed class RosterSymbol : SourceDeclaredSymbol, IRosterSymbol, INodeDeclaredSymbol<RosterNode>
{
    private ICatalogueSymbol? lazyGamesystem;

    public RosterSymbol(
        SourceGlobalNamespaceSymbol containingSymbol,
        RosterNode declaration,
        DiagnosticBag diagnostics) : base(containingSymbol, declaration)
    {
        ContainingNamespace = containingSymbol;
        Declaration = declaration;
        Costs = CreateCosts().ToImmutableArray();
        Forces = declaration.Forces.Select(x => new ForceSymbol(this, x, diagnostics)).ToImmutableArray();

        IEnumerable<RosterCostSymbol> CreateCosts()
        {
            foreach (var cost in declaration.Costs)
            {
                var limits = declaration.CostLimits.Where(x => x.TypeId == cost.TypeId).ToList();
                if (limits.Count > 1)
                {
                    diagnostics.Add(
                        ErrorCode.ERR_GenericError,
                        cost.GetLocation(),
                        symbols: ImmutableArray.Create<Symbol>(this),
                        args: "There are multiple Cost Limits with the TypeId of this Cost value.");
                }
                var limit = limits.FirstOrDefault();
                yield return new RosterCostSymbol(this, cost, limit, diagnostics);
            }
        }
    }

    public override RosterNode Declaration { get; }

    public override SourceGlobalNamespaceSymbol ContainingNamespace { get; }

    public override IModuleSymbol? ContainingModule => null;

    public override SymbolKind Kind => SymbolKind.Roster;

    public string? CustomNotes => Declaration.CustomNotes;

    public ICatalogueSymbol Gamesystem => GetBoundField(ref lazyGamesystem);

    public ImmutableArray<RosterCostSymbol> Costs { get; }

    public ImmutableArray<ForceSymbol> Forces { get; }

    ImmutableArray<IRosterCostSymbol> IRosterSymbol.Costs =>
        Costs.Cast<RosterCostSymbol, IRosterCostSymbol>();

    ImmutableArray<IForceSymbol> IForceContainerSymbol.Forces =>
        Forces.Cast<ForceSymbol, IForceSymbol>();

    public override void Accept(SymbolVisitor visitor) =>
        visitor.VisitRoster(this);

    public override TResult Accept<TResult>(SymbolVisitor<TResult> visitor) =>
        visitor.VisitRoster(this);

    public override TResult Accept<TArgument, TResult>(SymbolVisitor<TArgument, TResult> visitor, TArgument argument) =>
        visitor.VisitRoster(this, argument);

    protected override void BindReferencesCore(Binder binder, BindingDiagnosticBag diagnostics)
    {
        base.BindReferencesCore(binder, diagnostics);
        lazyGamesystem = binder.BindGamesystemSymbol(Declaration, diagnostics);
    }

    protected override ImmutableArray<Symbol> MakeAllMembers(BindingDiagnosticBag diagnostics) =>
        base.MakeAllMembers(diagnostics)
        .AddRange(Costs.Cast<RosterCostSymbol, Symbol>())
        .AddRange(Forces.Cast<ForceSymbol, Symbol>());

    internal override void ForceComplete(CancellationToken cancellationToken)
    {
        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var incompletePart = state.NextIncompletePart;
            switch (incompletePart)
            {
                case CompletionPart.None:
                    return;
                case CompletionPart.StartBindingReferences:
                case CompletionPart.FinishBindingReferences:
                    BindReferences();
                    break;
                case CompletionPart.Members:
                    GetMembersCore();
                    break;
                case CompletionPart.MembersCompleted:
                    {
                        var members = GetMembersCore();
                        foreach (var member in members)
                        {
                            cancellationToken.ThrowIfCancellationRequested();
                            member.ForceComplete(cancellationToken);
                        }
                        state.NotePartComplete(CompletionPart.MembersCompleted);
                        break;
                    }
                case CompletionPart.EvaluateModifiers:
                    // Modifier evaluation is performed inline during validation.
                    // This phase is reserved for future caching of effective values.
                    state.NotePartComplete(CompletionPart.EvaluateModifiers);
                    break;
                case CompletionPart.Validate:
                    {
                        // Ensure all catalogue symbols are fully completed before
                        // running validation. This prevents re-entrant ForceComplete calls
                        // from validation code that accesses lazy symbol properties (via
                        // GetBoundField → BindReferences → SpinWaitComplete), which previously
                        // caused process hang issues from SpinWait contention.
                        //
                        // We complete catalogues only (not the global namespace, which would
                        // recurse back into this RosterSymbol's ForceComplete). Catalogue
                        // symbols are the cross-graph references that validation accesses
                        // through the Binder — entry symbols, category links, constraints,
                        // query/effect symbols, etc. The roster's own member tree is already
                        // completed by the MembersCompleted phase above.
                        var compilation = (WhamCompilation)DeclaringCompilation;
                        foreach (var catalogue in compilation.SourceGlobalNamespace.Catalogues)
                        {
                            cancellationToken.ThrowIfCancellationRequested();
                            catalogue.ForceComplete(cancellationToken);
                        }
                        ConstraintValidator.Validate(
                            Declaration,
                            compilation,
                            compilation.DeclarationDiagnostics,
                            forceCatalogues: null,
                            cancellationToken);
                        state.NotePartComplete(CompletionPart.Validate);
                        break;
                    }
                default:
                    state.NotePartComplete(incompletePart);
                    break;
            }
            state.SpinWaitComplete(incompletePart, cancellationToken);
        }
    }
}
