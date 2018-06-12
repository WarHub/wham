using WarHub.ArmouryModel.Source;

namespace WarHub.ArmouryModel.ProjectModel
{
    public interface IDatafileInfo
    {
        /// <summary>
        /// Gets file path absolute or relative to root of the workspace.
        /// </summary>
        string Filepath { get; }

        /// <summary>
        /// Gets <see cref="SourceKind"/> of the data in the file.
        /// </summary>
        SourceKind DataKind { get; }

        /// <summary>
        /// Retrieves root <see cref="SourceNode"/> of the data file. May cause parsing.
        /// </summary>
        /// <returns>Retrieved root node.</returns>
        SourceNode GetData();

        /// <summary>
        /// Gets a name usable in file storage, with no extensions.
        /// </summary>
        /// <returns></returns>
        string GetStorageName();
    }

    public interface IDatafileInfo<out TData> : IDatafileInfo where TData : SourceNode
    {
        /// <summary>
        /// Type-parametrized version of <see cref="IDatafileInfo.GetData"/>.
        /// </summary>
        /// <returns>Retrieved root node.</returns>
        new TData GetData();
    }
}
