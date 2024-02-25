using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using WarHub.ArmouryModel.Source;

namespace WarHub.ArmouryModel.ProjectModel
{
    /// <summary>
    /// Contains information about a datafile and enables retrieval of
    /// deserialized content via <see cref="GetDataAsync"/>
    /// </summary>
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
        Task<SourceNode> GetDataAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Retrieves root <see cref="SourceNode"/> of the data file. May cause parsing.
        /// Blocking method.
        /// </summary>
        /// <returns>Retrieved root node.</returns>
        SourceNode GetData(CancellationToken cancellationToken = default);

        /// <summary>
        /// Attempts to retrieve root <see cref="SourceNode"/> of the data file
        /// without causing parsing. Returns true if successful.
        /// </summary>
        /// <param name="node">Retrieved node if method returned <see langword="true"/>, <see langword="null"/> otherwise.</param>
        /// <returns> <see langword="true"/> when successful, false otherwise.</returns>
        bool TryGetData([NotNullWhen(true)] out SourceNode? node);

        /// <summary>
        /// Gets a name usable in file storage, with no extensions.
        /// </summary>
        /// <returns></returns>
        string GetStorageName() => Path.GetFileNameWithoutExtension(Filepath);

        /// <summary>
        /// Create a tree for root node contained in this datafile.
        /// </summary>
        /// <remarks>
        /// Default implementation creates a lazy tree that will parse the data
        /// only when it's requested.
        /// </remarks>
        /// <returns>Created source tree.</returns>
        SourceTree CreateTree() => new LazyDatafileSourceTree(this);
    }
}
