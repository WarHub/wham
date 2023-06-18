using WarHub.ArmouryModel.Source;

namespace WarHub.ArmouryModel.ProjectModel
{
    /// <summary>
    /// Strongly typed datafile info instance with data available synchronously.
    /// </summary>
    /// <typeparam name="TNode">Type of SourceNode being stored in this file.</typeparam>
    public interface IDatafileInfo<out TNode> : IDatafileInfo where TNode : SourceNode
    {
        /// <summary>
        /// Type-parametrized and synchronous version of <see cref="IDatafileInfo.GetDataAsync"/>.
        /// </summary>
        /// <returns>Retrieved root node.</returns>
        TNode? Node { get; }
    }
}
