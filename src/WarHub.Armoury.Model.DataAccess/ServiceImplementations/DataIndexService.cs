// WarHub licenses this file to you under the MIT license.
// See LICENSE file in the project root for more information.

namespace WarHub.Armoury.Model.DataAccess.ServiceImplementations
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using PCLStorage;
    using Repo;

    public class DataIndexService : IDataIndexService
    {
        /// <summary>
        ///     Creates new instance and, starts loading index file except when Design mode is on or
        ///     <paramref name="autoStartLoadingIndex" /> is false.
        /// </summary>
        /// <param name="dataIndexStore">Data index service to save and load index to/from.</param>
        /// <param name="storageService">Storage service to index contents of.</param>
        /// <param name="repoManagerLocator">Repo manager locator to be updated with indexed systems.</param>
        /// <param name="repoStorageService">Repo storage service.</param>
        /// <param name="repoManagerFactory">Factory of repo managers.</param>
        /// <param name="log">Logger.</param>
        /// <param name="autoStartLoadingIndex">Set to false to stop auto loading index file.</param>
        public DataIndexService(
            IDataIndexStore dataIndexStore,
            IStorageService storageService,
            IRepoManagerLocator repoManagerLocator,
            IRepoStorageService repoStorageService,
            Func<ISystemIndex, IRepoManager> repoManagerFactory, ILog log, bool autoStartLoadingIndex = true)
        {
            if (dataIndexStore == null)
                throw new ArgumentNullException(nameof(dataIndexStore));
            if (storageService == null)
                throw new ArgumentNullException(nameof(storageService));
            if (repoManagerLocator == null)
                throw new ArgumentNullException(nameof(repoManagerLocator));
            if (repoStorageService == null)
                throw new ArgumentNullException(nameof(repoStorageService));
            if (repoManagerFactory == null)
                throw new ArgumentNullException(nameof(repoManagerFactory));
            DataIndexStore = dataIndexStore;
            RepoManagerLocator = repoManagerLocator;
            RepoManagerFactory = repoManagerFactory;
            Log = log;
            StorageService = storageService;
            repoStorageService.RepoChanged += OnRepoChanged;
            if (autoStartLoadingIndex)
            {
                LastIndexingTask = LoadIndexAsync();
            }
        }

        protected IDataIndexStore DataIndexStore { get; }

        protected DataIndex Index { get; } = new DataIndex();

        protected SystemIndex this[string gameSystemRawId]
        {
            get
            {
                return Index.SystemIndexes
                    .FirstOrDefault(x => x.GameSystemRawId.Equals(gameSystemRawId))
                    as SystemIndex ?? AddNewSystem(gameSystemRawId);
            }
        }

        protected SystemIndex this[CatalogueInfo catalogueInfo]
        {
            get
            {
                return Index.SystemIndexes
                    .FirstOrDefault(x => x.GameSystemRawId.Equals(catalogueInfo.GameSystemRawId))
                    as SystemIndex ?? AddNewSystem(catalogueInfo);
            }
        }

        protected SystemIndex this[RosterInfo rosterInfo]
        {
            get
            {
                return Index.SystemIndexes
                    .FirstOrDefault(x => x.GameSystemRawId.Equals(rosterInfo.GameSystemRawId))
                    as SystemIndex ?? AddNewSystem(rosterInfo);
            }
        }

        protected SystemIndex this[GameSystemInfo systemInfo]
        {
            get
            {
                return Index.SystemIndexes
                    .FirstOrDefault(x => x.GameSystemRawId.Equals(systemInfo.RawId))
                    as SystemIndex ?? AddNewSystem(systemInfo);
            }
        }

        protected ILog Log { get; }

        protected Func<ISystemIndex, IRepoManager> RepoManagerFactory { get; }

        protected IRepoManagerLocator RepoManagerLocator { get; }

        protected IStorageService StorageService { get; }

        public Task LastIndexingTask { get; protected set; } = Task.FromResult(0);

        public IObservableReadonlySet<ISystemIndex> SystemIndexes => Index.SystemIndexes;

        ISystemIndex IDataIndexService.this[GameSystemInfo systemInfo] => this[systemInfo];

        ISystemIndex IDataIndexService.this[CatalogueInfo catalogueInfo] => this[catalogueInfo];

        ISystemIndex IDataIndexService.this[RosterInfo rosterInfo] => this[rosterInfo];

        /// <summary>
        ///     Scans storage for files and creates index from results.
        /// </summary>
        /// <returns></returns>
        public virtual async Task IndexStorageAsync()
        {
            await (LastIndexingTask = DoIndexStorageAsync());
        }

        void IDataIndexService.OnRepoChanged(object sender, NotifyRepoChangedEventArgs e) => OnRepoChanged(sender, e);

        protected SystemIndex AddNewSystem(RosterInfo rosterInfo)
        {
            var systemIndex = new SystemIndex(rosterInfo.GameSystemRawId);
            Index.SystemIndexes.Add(systemIndex);
            return systemIndex;
        }

        protected SystemIndex AddNewSystem(CatalogueInfo catalogueInfo)
        {
            var systemIndex = new SystemIndex(catalogueInfo.GameSystemRawId);
            Index.SystemIndexes.Add(systemIndex);
            return systemIndex;
        }

        protected SystemIndex AddNewSystem(GameSystemInfo gameSystemInfo)
        {
            var systemIndex = new SystemIndex(gameSystemInfo);
            Index.SystemIndexes.Add(systemIndex);
            return systemIndex;
        }

        protected SystemIndex AddNewSystem(string gameSystemRawId)
        {
            var systemIndex = new SystemIndex(gameSystemRawId);
            Index.SystemIndexes.Add(systemIndex);
            return systemIndex;
        }

        /// <summary>
        ///     Copies over entries from provided index into this object's internal index. For each
        ///     system index a new manager is created and registered.
        /// </summary>
        /// <param name="dataIndex">Index to be copied.</param>
        protected void CopyAndRegister(DataIndex dataIndex)
        {
            foreach (var systemIndex in dataIndex.SystemIndexes)
            {
                var manager = RepoManagerFactory(systemIndex);
                RepoManagerLocator.Register(manager);
                Index.SystemIndexes.Add(systemIndex);
            }
        }

        /// <summary>
        ///     Scans through repo folders as provided by storage service. Creates and registers found
        ///     systems in this object's index.
        /// </summary>
        /// <returns></returns>
        protected virtual async Task DoIndexStorageAsync()
        {
            var repoFolders = await StorageService.GetGameSystemFoldersAsync();
            Index.SystemIndexes.Clear();
            foreach (var repoFolder in repoFolders)
            {
                await IndexSystemStorageAsync(repoFolder);
            }
            await IndexRostersStorageAsync();
            await SaveIndexAsync();
        }

        protected async Task LoadIndexAsync()
        {
            try
            {
                var dataIndex = await DataIndexStore.LoadItemAsync();
                CopyAndRegister(dataIndex);
            }
            catch (Exception e)
            {
                Log.Trace?.With("Loading index failed.", e);
                await IndexStorageAsync();
            }
        }

        protected async Task SaveIndexAsync()
        {
            try
            {
                await DataIndexStore.SaveItemAsync(Index);
            }
            catch (Exception e)
            {
                Log.Warn?.With("Saving index failed.", e);
            }
        }

        protected virtual async void OnRepoChanged(object sender, NotifyRepoChangedEventArgs e)
        {
            var rawId = e.SystemRawId;
            var systemIndex = this[rawId];
            switch (e.ChangeType)
            {
                case RepoChange.Removal:
                    e.VisitInfo(
                        gameSystemInfo =>
                        {
                            systemIndex.GameSystemInfo = null;
                            if (systemIndex.CatalogueInfos.Count == 0 && systemIndex.RosterInfos.Count == 0)
                            {
                                Index.SystemIndexes.Remove(systemIndex);
                            }
                        },
                        catalogueInfo => systemIndex.CatalogueInfos.Remove(catalogueInfo),
                        rosterInfo => systemIndex.RosterInfos.Remove(rosterInfo));
                    break;

                case RepoChange.Addition:
                case RepoChange.Update:
                    e.VisitInfo(
                        gameSystemInfo =>
                        {
                            systemIndex.GameSystemInfo = gameSystemInfo;
                            if (RepoManagerLocator.TryGetFor(gameSystemInfo) == null)
                            {
                                var manager = RepoManagerFactory(systemIndex);
                                RepoManagerLocator.Register(manager);
                            }
                        },
                        catalogueInfo =>
                        {
                            systemIndex.CatalogueInfos.Remove(catalogueInfo);
                            systemIndex.CatalogueInfos.Insert(0, catalogueInfo);
                        },
                        rosterInfo =>
                        {
                            systemIndex.RosterInfos.Remove(rosterInfo);
                            systemIndex.RosterInfos.Insert(0, rosterInfo);
                        });
                    break;
            }
            await SaveIndexAsync();
        }

        private async Task IndexRostersStorageAsync()
        {
            var rostersFolder = await StorageService.GetRostersFolderAsync();
            var rosterInfos = await StorageIndexer.IndexRosterInfosAsync(rostersFolder);
            foreach (var info in rosterInfos)
            {
                this[info].RosterInfos.Add(info);
            }
        }

        private async Task IndexSystemStorageAsync(IFolder repoFolder)
        {
            try
            {
                var gameSystemTuple = await StorageIndexer.IndexGameSystemInfoAsync(repoFolder);
                var catalogueTuples = await StorageIndexer.IndexCatalogueInfosAsync(repoFolder);
                var systemIndex = new SystemIndex(gameSystemTuple.Info);
                foreach (var catalogueTuple in catalogueTuples)
                {
                    systemIndex.CatalogueInfos.Add(catalogueTuple.Info);
                }
                Index.SystemIndexes.Add(systemIndex);
                var manager = RepoManagerFactory(systemIndex);
                RepoManagerLocator.Register(manager);
            }
            catch (StorageException e)
            {
                Log.Trace?.With("Indexing repo folder failed.", e);
            }
        }
    }
}
