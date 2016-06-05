// WarHub licenses this file to you under the MIT license.
// See LICENSE file in the project root for more information.

namespace WarHub.Armoury.Model.BattleScribe.Services
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using Repo;

    /// <summary>
    ///     Manages repository of BattleScribe formatted files concerning single game system. Provides
    ///     synchronous access to system, catalogues and rosters information, as well as asynchronous
    ///     access to actual content. May cache some of the loaded objects.
    /// </summary>
    public class BattleScribeRepoManager : IRepoManager
    {
        public BattleScribeRepoManager(ISystemIndex systemIndex, IRepoStorageService storageService)
        {
            if (systemIndex == null)
                throw new ArgumentNullException(nameof(systemIndex));
            if (storageService == null)
                throw new ArgumentNullException(nameof(storageService));
            SystemIndex = systemIndex;
            RepoStorageService = storageService;
        }

        protected IGameSystem GameSystem { get; set; }

        protected Dictionary<CatalogueInfo, ICatalogue> LoadedCatalogues { get; }
            = new Dictionary<CatalogueInfo, ICatalogue>();

        protected Dictionary<RosterInfo, IRoster> LoadedRosters { get; }
            = new Dictionary<RosterInfo, IRoster>();

        protected IRepoStorageService RepoStorageService { get; }

        protected GuidControllingSerializationService SerializationService { get; }
            = new GuidControllingSerializationService();

        public ISystemIndex SystemIndex { get; }

        public void ClearCache()
        {
            var rosterInfos = LoadedRosters.Keys.ToArray();
            foreach (var rosterInfo in rosterInfos)
            {
                RemoveRosterFromCache(rosterInfo);
            }
            var catalogueInfos = LoadedCatalogues.Keys.ToArray();
            foreach (var catalogueInfo in catalogueInfos)
            {
                RemoveCatalogueFromCache(catalogueInfo);
            }
            RemoveGameSystemFromCache();
        }

        public async Task<IRoster> CreateRosterAsync(RosterInfo rosterInfo)
        {
            CheckGameSystemGuid(rosterInfo);
            var system = await GetGameSystemAsync();
            var roster = RepoObjectFactory.CreateRoster(rosterInfo, system.Context);
            AddRosterToCache(roster, rosterInfo);
            return roster;
        }

        public async Task DeleteCatalogueAsync(CatalogueInfo catalogueInfo)
        {
            CheckGameSystemGuid(catalogueInfo);
            RemoveCatalogueFromCache(catalogueInfo);
            await RepoStorageService.DeleteCatalogueAsync(catalogueInfo);
        }

        public async Task DeleteRosterAsync(RosterInfo rosterInfo)
        {
            CheckGameSystemGuid(rosterInfo);
            RemoveRosterFromCache(rosterInfo);
            await RepoStorageService.DeleteRosterAsync(rosterInfo);
        }

        public async Task<ICatalogue> GetCatalogueAsync(CatalogueInfo catalogueInfo)
        {
            CheckGameSystemGuid(catalogueInfo);
            ICatalogue cachedCatalogue;
            if (LoadedCatalogues.TryGetValue(catalogueInfo, out cachedCatalogue))
            {
                return cachedCatalogue;
            }
            return await LoadCatalogueAsync(catalogueInfo);
        }

        public async Task<IGameSystem> GetGameSystemAsync()
        {
            return GameSystem ?? (GameSystem = await LoadGameSystemAsync());
        }

        public async Task<IRoster> GetRosterAsync(
            RosterInfo rosterInfo,
            bool isReadOnly = false,
            IProgress<LoadRosterProgressInfo> progress = null)
        {
            CheckGameSystemGuid(rosterInfo);
            IRoster cachedRoster;
            if (LoadedRosters.TryGetValue(rosterInfo, out cachedRoster))
            {
                return cachedRoster;
            }
            if (isReadOnly)
            {
                return await LoadRosterReadonlyAsync(rosterInfo);
            }
            if (SystemIndex.GameSystemInfo == null)
            {
                throw new GameSystemNotFoundException(rosterInfo);
            }
            return await LoadRosterAsync(rosterInfo, progress);
        }

        public async Task SaveRosterAsync(RosterInfo rosterInfo)
        {
            CheckGameSystemGuid(rosterInfo);
            IRoster roster;
            if (!LoadedRosters.TryGetValue(rosterInfo, out roster))
            {
                throw new RosterSavingException("roster not loaded.", rosterInfo);
            }
            using (var stream = await RepoStorageService.GetRosterOutputStreamAsync(rosterInfo))
            {
                SerializationService.SaveRoster(stream, roster);
            }
        }

        public async Task SaveCatalogueAsync(CatalogueInfo catalogueInfo)
        {
            CheckGameSystemGuid(catalogueInfo);
            ICatalogue catalogue;
            if (!LoadedCatalogues.TryGetValue(catalogueInfo, out catalogue))
            {
                throw new StorageException("Catalogue not loaded");
            }
            using (var stream = await RepoStorageService.GetCatalogueOutputStreamAsync(catalogueInfo))
            {
                SerializationService.SaveCatalogue(stream, catalogue);
            }
        }

        public async Task SaveGameSystemAsync(GameSystemInfo gameSystemInfo)
        {
            CheckGameSystemGuid(gameSystemInfo);
            if (GameSystem == null)
            {
                throw new StorageException("GameSystem not loaded");
            }
            using (var stream = await RepoStorageService.GetGameSystemOutputStreamAsync(gameSystemInfo))
            {
                SerializationService.SaveGameSystem(stream, GameSystem);
            }
        }

        protected virtual void AddRosterToCache(IRoster roster, RosterInfo rosterInfo)
        {
            LoadedRosters[rosterInfo] = roster;
        }

        protected virtual void RemoveCatalogueFromCache(CatalogueInfo catalogueInfo)
        {
            ICatalogue catalogue;
            if (LoadedCatalogues.TryGetValue(catalogueInfo, out catalogue))
            {
                catalogue.SystemContext = null;
                LoadedCatalogues.Remove(catalogueInfo);
            }
        }

        protected virtual void RemoveGameSystemFromCache()
        {
            if (GameSystem != null)
            {
                GameSystem = null;
            }
        }

        protected virtual void RemoveRosterFromCache(RosterInfo rosterInfo)
        {
            IRoster roster;
            if (LoadedRosters.TryGetValue(rosterInfo, out roster))
            {
                roster.SystemContext = null;
                LoadedRosters.Remove(rosterInfo);
            }
        }

        private void CheckGameSystemGuid(RosterInfo rosterInfo)
        {
            if (rosterInfo == null)
                throw new ArgumentNullException(nameof(rosterInfo));
            if (SystemIndex.GameSystemRawId != rosterInfo.GameSystemRawId)
            {
                throw new ArgumentException(
                    "Roster doesn't belong to this manager - wrong RepoManager!");
            }
        }

        private void CheckGameSystemGuid(CatalogueInfo catalogueInfo)
        {
            if (catalogueInfo == null)
                throw new ArgumentNullException(nameof(catalogueInfo));
            if (SystemIndex.GameSystemRawId != catalogueInfo.GameSystemRawId)
            {
                throw new ArgumentException(
                    "Catalogue doesn't belong to this manager - wrong RepoManager!");
            }
        }

        private void CheckGameSystemGuid(GameSystemInfo gameSystemInfo)
        {
            if (gameSystemInfo == null)
                throw new ArgumentNullException(nameof(gameSystemInfo));
            if (SystemIndex.GameSystemRawId != gameSystemInfo.RawId)
            {
                throw new ArgumentException(
                    "GameSystem doesn't belong to this manager - wrong RepoManager!");
            }
        }

        private async Task<ICatalogue> LoadCatalogueAsync(CatalogueInfo catalogueInfo)
        {
            // additionally requires game system to be loaded first
            var systemContext = (await GetGameSystemAsync()).Context;
            var catalogue = await RepoStorageService.LoadCatalogueAsync(catalogueInfo,
                SerializationService.LoadCatalogue);
            catalogue.SystemContext = systemContext;
            return LoadedCatalogues[catalogueInfo] = catalogue;
        }

        private async Task<IGameSystem> LoadGameSystemAsync()
        {
            return await RepoStorageService.LoadGameSystemAsync(SystemIndex.GameSystemInfo,
                SerializationService.LoadGameSystem);
        }

        private async Task<IRoster> LoadRosterAsync(RosterInfo rosterInfo,
            IProgress<LoadRosterProgressInfo> progress)
        {
            progress?.Report(new LoadRosterProgressInfo(LoadRosterState.Initiated));
            if (GameSystem == null)
            {
                progress?.Report(new LoadRosterProgressInfo(LoadRosterState.LoadingGameSystem));
                await GetGameSystemAsync();
            }
            progress?.Report(new LoadRosterProgressInfo(LoadRosterState.DeserializingRoster));
            LoadStreamedRosterAsyncCallback load = stream => LoadRosterFromStreamAsync(stream, progress);
            var roster = await RepoStorageService.LoadRosterAsync(rosterInfo, load);
            AddRosterToCache(roster, rosterInfo);
            return roster;
        }

        private async Task<IRoster> LoadRosterFromStreamAsync(Stream rosterXmlStream,
            IProgress<LoadRosterProgressInfo> progress)
        {
            var xmlRoster = SerializationService.DeserializeRoster(rosterXmlStream);
            // progress?.Report(new LoadRosterProgressInfo(LoadRosterState.IndexingRequiredCatalogues));
            var requiredCatalogueIds
                = GuidControllingSerializationService.ListRequiredCatalogueIds(xmlRoster);
            var loaded = 0;
            foreach (var catId in requiredCatalogueIds)
            {
                var catalogueInfo = SystemIndex.CatalogueInfos
                    .FirstOrDefault(info => info.RawId.Equals(catId, StringComparison.OrdinalIgnoreCase));
                if (catalogueInfo == null)
                {
                    throw new RequiredDataMissingException("Cannot open roster. One of the catalogues" +
                                                           $" is missing (catalogue id = '{catId}').");
                }
                progress?.Report(new LoadRosterProgressInfo(catalogueInfo.Name, loaded, requiredCatalogueIds.Count));
                await GetCatalogueAsync(catalogueInfo);
                ++loaded;
            }
            progress?.Report(new LoadRosterProgressInfo(LoadRosterState.PreparingRoster));
            SerializationService.GuidController.Process(xmlRoster);
            var roster = new Roster(xmlRoster) {SystemContext = GameSystem.Context};
            //progress?.Report(new LoadRosterProgressInfo(LoadRosterState.Finished));
            return roster;
        }

        private async Task<IRoster> LoadRosterReadonlyAsync(RosterInfo rosterInfo)
        {
            LoadStreamedRosterCallback load = SerializationService.LoadRosterReadonly;
            var roster = await RepoStorageService.LoadRosterAsync(rosterInfo, load);
            AddRosterToCache(roster, rosterInfo);
            return roster;
        }
    }
}
