using WarHub.ArmouryModel.Source;

namespace WarHub.ArmouryModel.ProjectSystem
{
    public interface IDatafileInfo
    {
        string Filepath { get; }

        SourceNode Data { get; }
    }

    public interface IDatafileInfo<out TData> : IDatafileInfo where TData : SourceNode
    {
        new TData Data { get; }
    }
}
