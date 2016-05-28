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
        IObservableReadonlySet<RemoteSource> SourceInfos { get; }

        void AddSource(RemoteSource info);

        Task<RemoteSourceDataIndex> DownloadIndexAsync(RemoteSource source);

        void RemoveSource(RemoteSource info);

        Task ReloadIndexAsync();
    }
}
