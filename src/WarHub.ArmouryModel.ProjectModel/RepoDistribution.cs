using System.Collections.Immutable;
using WarHub.ArmouryModel.Source;

namespace WarHub.ArmouryModel.ProjectModel
{
    /// <summary>
    /// This represents contents of the <c>.bsr</c> file.
    /// </summary>
    public record RepoDistribution(
        IDatafileInfo<DataIndexNode> Index,
        ImmutableArray<IDatafileInfo<CatalogueBaseNode>> Datafiles);
}
