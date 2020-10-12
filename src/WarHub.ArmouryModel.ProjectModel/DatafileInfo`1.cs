using System.IO;
using System.Threading.Tasks;
using WarHub.ArmouryModel.Source;

namespace WarHub.ArmouryModel.ProjectModel
{
    public sealed record DatafileInfo<TData>(string Filepath, TData Data) : IDatafileInfo<TData>
        where TData : SourceNode
    {
        public SourceKind DataKind => Data.Kind;

        public string GetStorageName() => Path.GetFileNameWithoutExtension(Filepath);

        public Task<SourceNode?> GetDataAsync() => Task.FromResult<SourceNode?>(Data);
    }
}
