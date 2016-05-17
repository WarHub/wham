// WarHub licenses this file to you under the MIT license.
// See LICENSE file in the project root for more information.

namespace WarHub.Armoury.Model.DataAccess
{
    using System;
    using System.Threading.Tasks;
    using Repo;

    /// <summary>
    ///     Provides access to a list of most recently used (opened, edited or saved) rosters, or more
    ///     accurately their RosterInfos.
    /// </summary>
    public interface IRostersService
    {
        /// <summary>
        ///     Lists top of the most recently used rosters sorted so that first is always newest.
        /// </summary>
        IObservableReadonlySet<RosterInfo> LastUsedRosters { get; }

        /// <summary>
        ///     Creates roster as per given setup and saves it in memory.
        /// </summary>
        /// <param name="rosterInfo">Basic roster details.</param>
        /// <returns>Handle for the new empty roster.</returns>
        Task<IRoster> CreateRosterAsync(RosterInfo rosterInfo);

        /// <summary>
        ///     Removes specified roster from storage, and from most recently used list.
        /// </summary>
        /// <param name="roster">Describes roster to be removed.</param>
        /// <returns></returns>
        Task DeleteRosterAsync(RosterInfo roster);

        /// <summary>
        ///     Loads existing roster from memory.
        /// </summary>
        /// <param name="rosterInfo">Identifies which roster should be loaded.</param>
        /// <param name="progress">Optional progress reporting sink.</param>
        /// <returns>Loaded roster.</returns>
        Task<IRoster> LoadAsync(RosterInfo rosterInfo, IProgress<LoadRosterProgressInfo> progress = null);

        /// <summary>
        ///     Saves previously loaded roster into memory.
        /// </summary>
        /// <param name="rosterInfo">Identifies which roster should be saved.</param>
        /// <returns></returns>
        Task SaveAsync(RosterInfo rosterInfo);
    }
}
