// WarHub licenses this file to you under the MIT license.
// See LICENSE file in the project root for more information.

namespace WarHub.Armoury.Model.DataAccess.ServiceImplementations
{
    using System;
    using System.Collections.Specialized;
    using System.Linq;
    using System.Threading.Tasks;
    using Repo;

    /// <summary>
    ///     Manages roster creation and provides convenient access layer to them, enclosing the use of
    ///     RepoManager in its methods.
    /// </summary>
    public class RostersService : IRostersService
    {
        public RostersService(
            IRepoManagerLocator repoManagerLocator,
            IStorageService storageService,
            IDataIndexService dataIndexService,
            IDispatcher dispatcher)
        {
            StorageService = storageService;
            RepoManagerLocator = repoManagerLocator;
            DataIndexService = dataIndexService;
            Dispatcher = dispatcher;
            var rosterInfos = DataIndexService.SystemIndexes
                .SelectMany(x => x.RosterInfos)
                .OrderBy(x => x.Name);
            RosterInfos = new ObservableList<RosterInfo>(rosterInfos);
            DataIndexService.SystemIndexes.CollectionChanged += OnSystemIndexesCollectionChanged;
            foreach (var index in DataIndexService.SystemIndexes)
            {
                index.RosterInfos.CollectionChanged += OnRosterInfosCollectionChanged;
            }
        }

        protected IDataIndexService DataIndexService { get; }

        protected IDispatcher Dispatcher { get; }

        protected IRepoManagerLocator RepoManagerLocator { get; }

        protected ObservableList<RosterInfo> RosterInfos { get; }

        protected IStorageService StorageService { get; }

        public IObservableReadonlySet<RosterInfo> LastUsedRosters => RosterInfos;

        public async Task<IRoster> CreateRosterAsync(RosterInfo rosterInfo)
        {
            var repoManager = RepoManagerLocator[rosterInfo];
            var roster = await repoManager.CreateRosterAsync(rosterInfo);
            await repoManager.SaveRosterAsync(rosterInfo);
            return roster;
        }

        public async Task DeleteRosterAsync(RosterInfo rosterInfo)
        {
            var manager = RepoManagerLocator.TryGetFor(rosterInfo);
            if (manager != null)
            {
                await manager.DeleteRosterAsync(rosterInfo);
            }
            else
            {
                await StorageService.DeleteRosterAsync(rosterInfo);
            }
            RosterInfos.Remove(rosterInfo);
        }

        public async Task<IRoster> LoadAsync(RosterInfo rosterInfo,
            IProgress<LoadRosterProgressInfo> progress = null)
        {
            if (rosterInfo == null)
                throw new ArgumentNullException(nameof(rosterInfo));
            var repoManager = RepoManagerLocator.TryGetFor(rosterInfo);
            if (repoManager == null)
            {
                throw new RosterLoadingException(
                    "roster not found in data index. Try refreshing the index in app settings.", rosterInfo);
            }
            var roster = await repoManager.GetRosterAsync(rosterInfo, false, progress);
            return roster;
        }

        public async Task SaveAsync(RosterInfo rosterInfo)
        {
            var repoManager = RepoManagerLocator[rosterInfo];
            await repoManager.SaveRosterAsync(rosterInfo);
        }

        private void OnRosterInfosCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            ReloadRosterList();
        }

        private void OnSystemIndexesCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                foreach (var index in e.NewItems.Cast<ISystemIndex>())
                {
                    index.RosterInfos.CollectionChanged += OnRosterInfosCollectionChanged;
                }
            }
            ReloadRosterList();
        }

        private void ReloadRosterList()
        {
            var rosters = DataIndexService.SystemIndexes
                .SelectMany(x => x.RosterInfos)
                .OrderBy(x => x.Name)
                .ToList();
            Dispatcher.InvokeOnUiAsync(() =>
            {
                RosterInfos.Clear();
                foreach (var item in rosters)
                {
                    RosterInfos.Add(item);
                }
            });
        }
    }
}
