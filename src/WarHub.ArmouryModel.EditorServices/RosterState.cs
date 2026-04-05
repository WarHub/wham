using WarHub.ArmouryModel.Concrete;
using WarHub.ArmouryModel.Source;

namespace WarHub.ArmouryModel.EditorServices;

/// <summary>
/// Represents a point-in-time roster together with dataset used to create the roster and roster diagnostics.
/// Diagnostics will contain warnings and errors to show to the user.
/// </summary>
public record RosterState(Compilation Compilation)
{
    public GamesystemNode Gamesystem => Compilation.GlobalNamespace.RootCatalogue.GetGamesystemDeclaration()
        ?? throw new InvalidOperationException();

    public ImmutableArray<CatalogueNode> Catalogues =>
        Compilation.GlobalNamespace.Catalogues.Where(x => !x.IsGamesystem)
            .Select(x => x.GetCatalogueDeclaration() ?? throw new InvalidOperationException())
            .ToImmutableArray();

    public IRosterSymbol? RosterSymbol => Compilation.GlobalNamespace.Rosters.SingleOrDefault();

    public RosterNode? Roster => RosterSymbol?.GetDeclaration();

    public RosterNode RosterRequired => Roster ?? throw new InvalidOperationException();

    public static RosterState CreateFromNodes(params SourceNode[] rootNodes)
        => CreateFromNodes((IEnumerable<SourceNode>)rootNodes);

    public static RosterState CreateFromNodes(IEnumerable<SourceNode> rootNodes)
    {
        var trees = rootNodes.Select(SourceTree.CreateForRoot).ToImmutableArray();
        var catalogueTrees = trees.Where(t => t.GetRoot() is not RosterNode).ToImmutableArray();
        var rosterTrees = trees.Where(t => t.GetRoot() is RosterNode).ToImmutableArray();
        if (rosterTrees.Length > 0 && catalogueTrees.Length > 0)
        {
            var catalogueCompilation = WhamCompilation.Create(catalogueTrees);
            return new(WhamCompilation.CreateRosterCompilation(rosterTrees, catalogueCompilation));
        }
        // No split needed: either all catalogues (no roster) or legacy mixed usage.
        return new(WhamCompilation.Create(trees));
    }

    public RosterState ReplaceRoster(RosterNode node)
    {
        var oldTree = RosterRequired.GetSourceTree(Compilation);
        var newTree = oldTree.WithRoot(node);
        return new(Compilation.ReplaceSourceTree(oldTree, newTree));
    }
}
