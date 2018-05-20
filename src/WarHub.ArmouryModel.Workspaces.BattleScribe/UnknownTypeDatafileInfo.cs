using WarHub.ArmouryModel.ProjectSystem;
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

        public SourceNode Data => null;
    }
}
