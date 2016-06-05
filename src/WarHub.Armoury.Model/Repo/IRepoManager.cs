// WarHub licenses this file to you under the MIT license.
// See LICENSE file in the project root for more information.

namespace WarHub.Armoury.Model.Repo
{
    using System;
    using System.Threading.Tasks;

    /// <summary>
    ///     Responsible for management of single game system repository. That means it manages one game
    ///     system, catalogues for this system, and all rosters created with them. Access to these
    ///     objects is asynchronous, because altough it references all objects available, they are
    ///     loaded only on first use.
    /// </summary>
    public interface IRepoManager
    {
        /// <summary>
        ///     Provides access to all Info objects of this manager. May be null if manager was created
        ///     without assigned game system.
        /// </summary>
        ISystemIndex SystemIndex { get; }

        /// <summary>
        ///     Removes any cached items if implementation has cache.
        /// </summary>
        void ClearCache();

        /// <summary>
        ///     Creates new roster according to provided description.
        /// </summary>
        /// <param name="rosterInfo">Describes basic properties of requested roster.</param>
        /// <returns>Created roster.</returns>
        /// <exception cref="ArgumentException">
        ///     If described roster's game system is not same as this manager's.
        /// </exception>
        /// <exception cref="ArgumentNullException">If argument is null.</exception>
        Task<IRoster> CreateRosterAsync(RosterInfo rosterInfo);

        /// <summary>
        ///     Premanently deletes specified catalogue from manager's list as well as from storage.
        /// </summary>
        /// <param name="catalogueInfo">Describes the catalogue to be removed.</param>
        /// <returns></returns>
        Task DeleteCatalogueAsync(CatalogueInfo catalogueInfo);

        /// <summary>
        ///     Premanently deletes specified roster from manager's list as well as from storage.
        /// </summary>
        /// <param name="rosterInfo">Describes the roster to be removed.</param>
        /// <returns></returns>
        Task DeleteRosterAsync(RosterInfo rosterInfo);

        /// <summary>
        ///     Catalogue described by <paramref name="catalogueInfo" /> is loaded (or not if cached) and returned.
        /// </summary>
        /// <param name="catalogueInfo">Identifies catalogue to return.</param>
        /// <returns>Catalogue described by provided info.</returns>
        /// <exception cref="ArgumentNullException">If argument is null.</exception>
        Task<ICatalogue> GetCatalogueAsync(CatalogueInfo catalogueInfo);

        /// <summary>
        ///     Game system which is represented by this manager is loaded (or not if cached) and returned.
        /// </summary>
        /// <returns>Game System of this repo.</returns>
        Task<IGameSystem> GetGameSystemAsync();

        /// <summary>
        ///     Finds roster described by provided info. If not cached, the roster will be loaded.
        ///     According to the demanded mode, the roster will be linked to referenced catalogues and
        ///     game system or not (in readonly mode). If it will, then required catalogues and game
        ///     system will be loaded automatically (or if cached, found and used).
        /// </summary>
        /// <param name="rosterInfo">Identifies the roster to return.</param>
        /// <param name="isReadOnly">
        ///     If true, catalogues and game system won't be linked. If false (default), required
        ///     objects will be linked. If unavailable, exceptions will be thrown.
        /// </param>
        /// <param name="progress">Allows for reporting progress of loading the roster.</param>
        /// <returns>Roster described by provided info.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="rosterInfo" /> is null.</exception>
        Task<IRoster> GetRosterAsync(RosterInfo rosterInfo, bool isReadOnly = false,
            IProgress<LoadRosterProgressInfo> progress = null);

        /// <summary>
        ///     Saves described roster in its current state.
        /// </summary>
        /// <param name="rosterInfo">Info describing roster to be saved.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">If argument is null.</exception>
        Task SaveRosterAsync(RosterInfo rosterInfo);

        /// <summary>
        ///     Saves described catalogue in its current state.
        /// </summary>
        /// <param name="catalogueInfo">Info describing catalogue to be saved.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">If argument is null.</exception>
        Task SaveCatalogueAsync(CatalogueInfo catalogueInfo);

        /// <summary>
        ///     Saves described game system in its current state.
        /// </summary>
        /// <param name="gameSystemInfo">Info describing game system to be saved.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">If argument is null.</exception>
        Task SaveGameSystemAsync(GameSystemInfo gameSystemInfo);
    }
}
