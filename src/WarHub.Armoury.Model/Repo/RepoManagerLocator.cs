// WarHub licenses this file to you under the MIT license.
// See LICENSE file in the project root for more information.

namespace WarHub.Armoury.Model.Repo
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    ///     Default implementation of <see cref="IRepoManagerLocator" /> fulfilling exact contract.
    /// </summary>
    public class RepoManagerLocator : IRepoManagerLocator
    {
        protected Dictionary<string, IRepoManager> ManagerDict { get; } = new Dictionary<string, IRepoManager>();

        public IEnumerable<GameSystemInfo> ListSystems => from manager in ManagerDict.Values
                                                          let systemInfo = manager.SystemIndex.GameSystemInfo
                                                          where systemInfo != null
                                                          select systemInfo;

        public IRepoManager this[RosterInfo roster] => ManagerDict[roster.GameSystemRawId];

        public IRepoManager this[CatalogueInfo catalogue] => ManagerDict[catalogue.GameSystemRawId];

        public IRepoManager this[GameSystemInfo gameSystem] => ManagerDict[gameSystem.RawId];

        public void Deregister(IRepoManager manager)
        {
            if (manager == null)
                throw new ArgumentNullException(nameof(manager));
            ManagerDict.Remove(manager.SystemIndex.GameSystemRawId);
        }

        public void Register(IRepoManager manager)
        {
            if (manager == null)
                throw new ArgumentNullException(nameof(manager));
            ManagerDict[manager.SystemIndex.GameSystemRawId] = manager;
        }

        public IRepoManager TryGetFor(RosterInfo roster)
        {
            IRepoManager manager;
            ManagerDict.TryGetValue(roster.GameSystemRawId, out manager);
            return manager;
        }

        public IRepoManager TryGetFor(CatalogueInfo catalogue)
        {
            IRepoManager manager;
            ManagerDict.TryGetValue(catalogue.GameSystemRawId, out manager);
            return manager;
        }

        public IRepoManager TryGetFor(GameSystemInfo gameSystem)
        {
            IRepoManager manager;
            ManagerDict.TryGetValue(gameSystem.RawId, out manager);
            return manager;
        }
    }
}
