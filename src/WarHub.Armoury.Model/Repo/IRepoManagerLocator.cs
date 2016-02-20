// WarHub licenses this file to you under the MIT license.
// See LICENSE file in the project root for more information.

namespace WarHub.Armoury.Model.Repo
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    ///     Provides registry of- and convenient access to game-system-specific
    ///     <see cref="IRepoManager" /> s.
    /// </summary>
    public interface IRepoManagerLocator
    {
        /// <summary>
        ///     Returns Manager for game system which is used by roster.
        /// </summary>
        /// <param name="rosterInfo">Provides info on game system used by roster.</param>
        /// <returns>Manager of appropriate game system.</returns>
        /// <exception cref="KeyNotFoundException">
        ///     When there is no manager for the queried system.
        /// </exception>
        /// <exception cref="NullReferenceException">When argument is null.</exception>
        IRepoManager this[RosterInfo rosterInfo] { get; }

        /// <summary>
        ///     Returns Manager for game system which is used by catalogue.
        /// </summary>
        /// <param name="catalogueInfo">Provides info on game system used by catalogue.</param>
        /// <returns>Manager of appropriate game system.</returns>
        /// <exception cref="KeyNotFoundException">
        ///     When there is no manager for the queried system.
        /// </exception>
        /// <exception cref="NullReferenceException">When argument is null.</exception>
        IRepoManager this[CatalogueInfo catalogueInfo] { get; }

        /// <summary>
        ///     Returns Manager for game system.
        /// </summary>
        /// <param name="gameSystemInfo">Provides info on game system.</param>
        /// <returns>Manager of appropriate game system.</returns>
        /// <exception cref="KeyNotFoundException">
        ///     When there is no manager for the queried system.
        /// </exception>
        /// <exception cref="NullReferenceException">When argument is null.</exception>
        IRepoManager this[GameSystemInfo gameSystemInfo] { get; }

        /// <summary>
        ///     Lists all systems for which a Manager is registered. The list may be updated during
        ///     program execution. The list doesn't contain managers with no
        ///     <see cref="GameSystemInfo" /> assigned - these managers are only accessible through
        ///     <see cref="TryGetFor(RosterInfo)" /> if created and registered at all.
        /// </summary>
        IEnumerable<GameSystemInfo> ListSystems { get; }

        /// <summary>
        ///     Removes manager from registry. It no longer will be accessible through this object.
        /// </summary>
        /// <param name="manager">The manager to be removed.</param>
        void Deregister(IRepoManager manager);

        /// <summary>
        ///     Adds the manager to internal collection if it wasn't added already. That manager is
        ///     identified by provided its SystemIndex.GameSystemRawId. If a manager with this game
        ///     system ID exists, it's replaced with new one.
        /// </summary>
        /// <param name="manager">The manager to register.</param>
        /// <exception cref="ArgumentNullException">When <paramref name="manager" /> is null.</exception>
        void Register(IRepoManager manager);

        /// <summary>
        ///     Returns Manager for game system which is used by roster.
        /// </summary>
        /// <param name="rosterInfo">Provides info on game system used by roster.</param>
        /// <returns>Manager of appropriate game system or null if none is registered.</returns>
        /// <exception cref="NullReferenceException">When argument is null.</exception>
        IRepoManager TryGetFor(RosterInfo rosterInfo);

        /// <summary>
        ///     Returns Manager for game system which is used by catalogue.
        /// </summary>
        /// <param name="catalogueInfo">Provides info on game system used by catalogue.</param>
        /// <returns>Manager of appropriate game system or null if none is registered.</returns>
        /// <exception cref="NullReferenceException">When argument is null.</exception>
        IRepoManager TryGetFor(CatalogueInfo catalogueInfo);

        /// <summary>
        ///     Returns Manager for game system.
        /// </summary>
        /// <param name="gameSystemInfo">Provides info on game system.</param>
        /// <returns>Manager of appropriate game system or null if none is registered.</returns>
        /// <exception cref="NullReferenceException">When argument is null.</exception>
        IRepoManager TryGetFor(GameSystemInfo gameSystemInfo);
    }
}
