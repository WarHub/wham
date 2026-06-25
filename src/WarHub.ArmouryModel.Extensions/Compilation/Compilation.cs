using WarHub.ArmouryModel.Source;

namespace WarHub.ArmouryModel;

public abstract class Compilation
{
    internal static string NoCategorySymbolId => "(No Category)";

    internal Compilation(
        string? name,
        ImmutableArray<SourceTree> sourceTrees,
        CompilationOptions options)
    {
        Name = name;
        SourceTrees = sourceTrees;
        Options = options;
    }

    public string? Name { get; }

    public ImmutableArray<SourceTree> SourceTrees { get; }

    public CompilationOptions Options { get; }

    /// <summary>
    /// The referenced catalogue compilation whose symbols are visible from this compilation,
    /// or <see langword="null"/> for catalogue compilations.
    /// <para>
    /// A <b>catalogue compilation</b> has no catalogue reference and contains only catalogue/gamesystem trees.
    /// A <b>roster compilation</b> references exactly one catalogue compilation and contains only roster trees.
    /// </para>
    /// </summary>
    public virtual Compilation? CatalogueReference => null;

    /// <summary>
    /// <see langword="true"/> when this is a roster compilation (has a catalogue reference).
    /// </summary>
    public bool HasCatalogueReference => CatalogueReference is not null;

    /// <summary>
    /// Gets all source trees, including those from the <see cref="CatalogueReference"/>.
    /// For catalogue compilations, this is the same as <see cref="SourceTrees"/>.
    /// For roster compilations, this includes both the catalogue's trees and the roster's own trees.
    /// </summary>
    public ImmutableArray<SourceTree> AllSourceTrees =>
        CatalogueReference is { } catRef
            ? catRef.SourceTrees.AddRange(SourceTrees)
            : SourceTrees;

    public abstract IGamesystemNamespaceSymbol GlobalNamespace { get; }

    public abstract ICategoryEntrySymbol NoCategoryEntrySymbol { get; }

    public abstract SemanticModel GetSemanticModel(SourceTree tree);

    public abstract ImmutableArray<Diagnostic> GetDiagnostics(CancellationToken cancellationToken = default);

    public abstract ImmutableArray<Diagnostic> GetDeclarationDiagnostics(CancellationToken cancellationToken = default);

    public abstract ImmutableArray<Diagnostic> GetConstraintDiagnostics(CancellationToken cancellationToken = default);

    public abstract Compilation AddSourceTrees(params SourceTree[] trees);

    /// <summary>
    /// Adds roster trees to this compilation.
    /// If this is a catalogue compilation, creates a new roster compilation referencing
    /// this as the catalogue. If this is already a roster compilation, adds to the
    /// existing roster trees.
    /// </summary>
    public abstract Compilation AddRosterTrees(params SourceTree[] rosterTrees);

    public abstract Compilation ReplaceSourceTree(SourceTree oldTree, SourceTree? newTree);

    /// <summary>
    /// Finds the <see cref="SourceTree"/> whose root matches <paramref name="rootNode"/>,
    /// searching own source trees first, then the <see cref="CatalogueReference"/> if present.
    /// Returns <see langword="null"/> if not found.
    /// </summary>
    public virtual SourceTree? FindSourceTree(SourceNode rootNode)
    {
        var tree = SourceTrees.SingleOrDefault(x => x.GetRoot() == rootNode);
        if (tree is not null)
            return tree;
        return CatalogueReference?.FindSourceTree(rootNode);
    }

    /// <summary>
    /// Resolves a <see cref="SymbolKey"/> in this compilation, returning the matching symbol
    /// or a description of why resolution failed (missing or ambiguous).
    /// </summary>
    public abstract SymbolKeyResolution ResolveSymbolKey(SymbolKey key);

    internal abstract ICatalogueSymbol CreateMissingGamesystemSymbol(DiagnosticBag diagnostics);
}
