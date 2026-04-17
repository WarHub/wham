using WarHub.ArmouryModel.Concrete;

namespace WarHub.ArmouryModel.EditorServices;

/// <summary>
/// Lazily manages a per-roster <see cref="WhamCompilation"/> that references a shared catalogue compilation.
/// Immutable fork pattern: <see cref="WithRosterTree"/> and <see cref="WithCatalogueCompilation"/>
/// return new instances with invalidated cache.
/// </summary>
internal sealed class CompilationTracker
{
    private WhamCompilation? cachedCompilation;

    public CompilationTracker(SourceTree rosterTree, WhamCompilation catalogueCompilation)
    {
        RosterTree = rosterTree;
        CatalogueCompilation = catalogueCompilation;
    }

    public SourceTree RosterTree { get; }

    public WhamCompilation CatalogueCompilation { get; }

    /// <summary>
    /// Gets or lazily creates the roster compilation. Thread-safe via Interlocked.
    /// </summary>
    public WhamCompilation GetCompilation()
    {
        var compilation = Volatile.Read(ref cachedCompilation);
        if (compilation is not null)
        {
            return compilation;
        }
        var newCompilation = WhamCompilation.CreateRosterCompilation([RosterTree], CatalogueCompilation);
        Interlocked.CompareExchange(ref cachedCompilation, newCompilation, null);
        return Volatile.Read(ref cachedCompilation)!;
    }

    /// <summary>
    /// Creates a new tracker with an updated roster tree. The cached compilation is invalidated.
    /// </summary>
    public CompilationTracker WithRosterTree(SourceTree newTree) => new(newTree, CatalogueCompilation);

    /// <summary>
    /// Creates a new tracker with an updated catalogue compilation. The cached compilation is invalidated.
    /// </summary>
    public CompilationTracker WithCatalogueCompilation(WhamCompilation newCatComp) => new(RosterTree, newCatComp);
}
