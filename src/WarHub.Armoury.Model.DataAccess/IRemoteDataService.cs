// WarHub licenses this file to you under the MIT license.
// See LICENSE file in the project root for more information.

namespace WarHub.Armoury.Model.DataAccess
{
    using System.Threading.Tasks;
    using Repo;

    /// <summary>
    ///     Provides methods to access remote source indexes, check for updates using them and download chosen updates.
    /// </summary>
    public interface IRemoteDataService
    {
        IObservableReadonlySet<RemoteDataSourceInfo> SourceInfos { get; }

        void AddSource(RemoteDataSourceInfo info);

        Task<RemoteDataSourceIndex> DownloadIndexAsync(RemoteDataSourceInfo source);

        void RemoveSource(RemoteDataSourceInfo info);
    }
}
