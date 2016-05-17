// WarHub licenses this file to you under the MIT license.
// See LICENSE file in the project root for more information.

namespace WarHub.Armoury.Model.DataAccess.ServiceImplementations
{
    using System;
    using System.Linq;
    using System.Net.Http;
    using System.Threading.Tasks;
    using BattleScribe.Files;
    using Repo;

    public class RemoteDataService : IRemoteDataService
    {
        public RemoteDataService(IRemoteDataSettings remoteDataSettings)
        {
            if (remoteDataSettings == null)
                throw new ArgumentNullException(nameof(remoteDataSettings));
            DataSettings = remoteDataSettings;
            SourceInfos = new ObservableList<RemoteDataSourceInfo>(DataSettings.Entries);
        }

        protected IRemoteDataSettings DataSettings { get; }

        protected ObservableList<RemoteDataSourceInfo> SourceInfos { get; }

        IObservableReadonlySet<RemoteDataSourceInfo> IRemoteDataService.SourceInfos => SourceInfos;

        public void AddSource(RemoteDataSourceInfo info)
        {
            DataSettings.AddEntry(info);
            if (SourceInfos.Any(x => x.IndexUri == info.IndexUri))
            {
                var duplicates = SourceInfos.Where(x => x.IndexUri == info.IndexUri);
                foreach (var duplicate in duplicates.ToList())
                {
                    SourceInfos.Remove(duplicate);
                }
            }
            SourceInfos.Add(info);
        }

        public virtual async Task<RemoteDataSourceIndex> DownloadIndexAsync(RemoteDataSourceInfo source)
            => await DownloadIndexStaticAsync(source);

        public void RemoveSource(RemoteDataSourceInfo info)
        {
            SourceInfos.Remove(info);
            DataSettings.RemoveEntry(info);
        }

        public static async Task<RemoteDataSourceIndex> DownloadIndexStaticAsync(RemoteDataSourceInfo source)
        {
            var url = new Uri(source.IndexUri);
            using (var client = new HttpClient())
            using (var response = await client.GetAsync(url))
            {
                response.EnsureSuccessStatusCode();
                using (var contentStream = await response.Content.ReadAsStreamAsync())
                {
                    return DataIndexFile.ReadBattleScribeIndexAuto(source.IndexUri, contentStream);
                }
            }
        }

        private static string GetFilename(Uri uri)
        {
            var lastSegment = uri.Segments.Last();
            return lastSegment;
        }
    }
}
