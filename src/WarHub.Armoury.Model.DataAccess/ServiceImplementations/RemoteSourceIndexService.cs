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

    public class RemoteSourceIndexService : IRemoteSourceIndexService
    {
        public RemoteSourceIndexService(IRemoteSourceIndexStore indexStore, ILog log)
        {
            IndexStore = indexStore;
            Log = log;
        }

        protected RemoteSourceIndex Index { get; } = new RemoteSourceIndex();

        protected IRemoteSourceIndexStore IndexStore { get; }

        protected ILog Log { get; }

        protected ObservableList<RemoteSource> SourceInfos => Index.RemoteSources;

        IObservableReadonlySet<RemoteSource> IRemoteSourceIndexService.SourceInfos => Index.RemoteSources;

        public async void AddSource(RemoteSource info)
        {
            if (SourceInfos.Any(x => x.IndexUri == info.IndexUri))
            {
                var duplicates = SourceInfos.Where(x => x.IndexUri == info.IndexUri).ToArray();
                foreach (var duplicate in duplicates)
                {
                    SourceInfos.Remove(duplicate);
                }
            }
            SourceInfos.Add(info);
            await SaveIndexAsync();
        }

        public virtual async Task<RemoteSourceDataIndex> DownloadIndexAsync(RemoteSource source)
            => await DownloadIndexStaticAsync(source);

        public async void RemoveSource(RemoteSource info)
        {
            SourceInfos.Remove(info);
            await SaveIndexAsync();
        }

        public async Task ReloadIndexAsync()
        {
            SourceInfos.Clear();
            await LoadIndexAsync();
        }

        protected async Task LoadIndexAsync()
        {
            try
            {
                var index = await IndexStore.LoadItemAsync();
                Copy(index);
                Log.Trace?.With("Loading index succeeded.");
            }
            catch (Exception e)
            {
                Log.Trace?.With("Loading index failed.", e);
            }
        }

        protected async Task SaveIndexAsync()
        {
            try
            {
                await IndexStore.SaveItemAsync(Index);
                Log.Trace?.With("Saving index succeeded.");
            }
            catch (Exception e)
            {
                Log.Warn?.With("Saving index failed.", e);
            }
        }

        private void Copy(RemoteSourceIndex index)
        {
            foreach (var remoteSource in index.RemoteSources)
            {
                SourceInfos.Add(remoteSource);
            }
        }

        public static async Task<RemoteSourceDataIndex> DownloadIndexStaticAsync(RemoteSource source)
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
    }
}
