using System.IO;
using Amadevus.RecordGenerator;
using WarHub.ArmouryModel.Source;

namespace WarHub.ArmouryModel.ProjectModel
{
    [Record]
    public sealed partial class UnknownTypeDatafileInfo : IDatafileInfo, IDatafileInfo<SourceNode>
    {
        public string Filepath { get; }

        public SourceKind DataKind => SourceKind.Unknown;

        public SourceNode GetData() => null;

        public string GetStorageName() => Path.GetFileNameWithoutExtension(Filepath);
    }
}
