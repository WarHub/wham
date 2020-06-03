using System.IO;
using System.Threading.Tasks;
using Amadevus.RecordGenerator;
using WarHub.ArmouryModel.Source;

namespace WarHub.ArmouryModel.ProjectModel
{
    [Record]
    public sealed partial class UnknownTypeDatafileInfo : IDatafileInfo, IDatafileInfo<SourceNode>
    {
        private static readonly Task<SourceNode?> nullTask = Task.FromResult<SourceNode?>(null);

        public string Filepath { get; }

        public SourceKind DataKind => SourceKind.Unknown;

        public SourceNode? Data => null;

        public Task<SourceNode?> GetDataAsync() => nullTask;

        public string GetStorageName() => Path.GetFileNameWithoutExtension(Filepath);
    }
}
