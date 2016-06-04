// WarHub licenses this file to you under the MIT license.
// See LICENSE file in the project root for more information.

namespace WarHub.Armoury.Model.DataAccess
{
    using System.Threading.Tasks;
    using Repo;

    /// <summary>
    ///     Provides methods to access remote source indexes, check for updates using them and download chosen updates.
    /// </summary>
    public interface IRemoteSourceIndexService
    {
        /// <summary>
        ///     Gets last indexing task created. If no task was run yet (because ie. index was
        ///     succesfully loaded from file), this is an empty completed task.
        /// </summary>
        Task LastIndexingTask { get; }

        IObservableReadonlySet<RemoteSource> SourceInfos { get; }

        void AddSource(RemoteSource info);

        Task<RemoteSourceDataIndex> DownloadIndexAsync(RemoteSource source);

        void RemoveSource(RemoteSource info);

        Task ReloadIndexAsync();
    }
}
