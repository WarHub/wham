using System.Collections.Immutable;
using WarHub.ArmouryModel.Source;

namespace WarHub.ArmouryModel.ProjectModel
{
    /// <summary>
    /// This represents contents of the <c>.bsr</c> file.
    /// </summary>
    [Record]
    public partial class RepoDistribution
    {
        public IDatafileInfo<DataIndexNode> Index { get; }

        public ImmutableArray<IDatafileInfo<CatalogueBaseNode>> Datafiles { get; }
    }
}
