using System.IO;
using System.Threading.Tasks;
using Amadevus.RecordGenerator;
using WarHub.ArmouryModel.Source;

namespace WarHub.ArmouryModel.ProjectModel
{
    [Record]
    public sealed partial class DatafileInfo<TData> : IDatafileInfo<TData>
        where TData : SourceNode
    {
        // TODO internal ctor

        public string Filepath { get; }

        public TData Data { get; }

        public SourceKind DataKind => Data.Kind;

        public string GetStorageName() => Path.GetFileNameWithoutExtension(Filepath);

        public Task<SourceNode?> GetDataAsync() => Task.FromResult<SourceNode?>(Data);
    }
}
