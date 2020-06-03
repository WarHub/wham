using WarHub.ArmouryModel.Source;

namespace WarHub.ArmouryModel.ProjectModel
{
    /// <summary>
    /// Strongly typed datafile info instance with data available synchronously.
    /// </summary>
    /// <typeparam name="TData"></typeparam>
    public interface IDatafileInfo<out TData> : IDatafileInfo where TData : SourceNode
    {
        /// <summary>
        /// Type-parametrized and synchronous version of <see cref="IDatafileInfo.GetDataAsync"/>.
        /// </summary>
        /// <returns>Retrieved root node.</returns>
        TData? Data { get; }
    }
}
