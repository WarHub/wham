using WarHub.ArmouryModel.Source;

namespace WarHub.ArmouryModel.ProjectModel
{
    public interface IDatafileInfo
    {
        string Filepath { get; }

        SourceKind DataKind { get; }

        SourceNode GetData();
    }

    public interface IDatafileInfo<out TData> : IDatafileInfo where TData : SourceNode
    {
        new TData GetData();
    }
}
