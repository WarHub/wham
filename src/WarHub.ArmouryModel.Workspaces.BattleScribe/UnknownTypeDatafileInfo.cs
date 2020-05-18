using System.IO;
using WarHub.ArmouryModel.ProjectModel;
using WarHub.ArmouryModel.Source;

namespace WarHub.ArmouryModel.Workspaces.BattleScribe
{
    internal class UnknownTypeDatafileInfo : IDatafileInfo
    {
        public UnknownTypeDatafileInfo(string filepath)
        {
            Filepath = filepath;
        }

        public string Filepath { get; }

        public SourceKind DataKind => SourceKind.Unknown;

        public SourceNode? GetData() => null;

        public string GetStorageName() => Path.GetFileNameWithoutExtension(Filepath);
    }
}
