using System.IO;
using System.Threading.Tasks;
using WarHub.ArmouryModel.Source;

namespace WarHub.ArmouryModel.ProjectModel
{
    public record UnknownTypeDatafileInfo(string Filepath) : IDatafileInfo, IDatafileInfo<SourceNode>
    {
        private static readonly Task<SourceNode?> nullTask = Task.FromResult<SourceNode?>(null);

        public SourceKind DataKind => SourceKind.Unknown;

        public SourceNode? Data => null;

        public Task<SourceNode?> GetDataAsync() => nullTask;

        public string GetStorageName() => Path.GetFileNameWithoutExtension(Filepath);
    }
}
