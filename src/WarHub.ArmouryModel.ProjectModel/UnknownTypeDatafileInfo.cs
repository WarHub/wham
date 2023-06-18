using System.IO;
using System.Threading;
using System.Threading.Tasks;
using WarHub.ArmouryModel.Source;

namespace WarHub.ArmouryModel.ProjectModel
{
    public record UnknownTypeDatafileInfo(string Filepath) : IDatafileInfo, IDatafileInfo<SourceNode>
    {
        private static readonly Task<SourceNode?> nullTask = Task.FromResult<SourceNode?>(null);

        public SourceKind DataKind => SourceKind.Unknown;

        public SourceNode? Node => null;

        public SourceNode? GetData(CancellationToken cancellationToken = default) => null;

        public Task<SourceNode?> GetDataAsync(CancellationToken cancellationToken = default) => nullTask;

        public string GetStorageName() => Path.GetFileNameWithoutExtension(Filepath);

        public bool TryGetData(out SourceNode? node)
        {
            node = null;
            return true;
        }
    }
}
