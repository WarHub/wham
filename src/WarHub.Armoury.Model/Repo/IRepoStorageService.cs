// WarHub licenses this file to you under the MIT license.
// See LICENSE file in the project root for more information.

namespace WarHub.Armoury.Model.Repo
{
    using System;
    using System.IO;
    using System.Threading.Tasks;

    public delegate ICatalogue LoadStreamedCatalogueCallback(Stream stream);

    public delegate IGameSystem LoadStreamedGameSystemCallback(Stream stream);

    public delegate Task<IRoster> LoadStreamedRosterAsyncCallback(Stream stream);

    public delegate IRoster LoadStreamedRosterCallback(Stream stream);

    /// <summary>
    ///     Provides access to streams of repo data objects using callback functions.
    /// </summary>
    public interface IRepoStorageService : INotifyRepoChanged
    {
        /// <summary>
        ///     Deletes specified catalogue permanently from storage.
        /// </summary>
        /// <param name="info">Describes the catalogue to be removed.</param>
        /// <returns></returns>
        Task DeleteCatalogueAsync(CatalogueInfo info);

        /// <summary>
        ///     Deletes specified game system permanently from storage.
        /// </summary>
        /// <param name="info">Describes the game system to be removed.</param>
        /// <returns></returns>
        Task DeleteGameSystemAsync(GameSystemInfo info);

        /// <summary>
        ///     Deletes specified roster permanently from storage.
        /// </summary>
        /// <param name="info">Describes the roster to be removed.</param>
        /// <returns></returns>
        Task DeleteRosterAsync(RosterInfo info);

        /// <summary>
        ///     Invokes provided callback with opened stream described by
        ///     <paramref name="info" /> .
        /// </summary>
        /// <param name="info">Identifies the catalogue to be opened.</param>
        /// <param name="loadCallback">Loads catalogue from provided stream.</param>
        /// <returns>Result of <paramref name="loadCallback" /> .</returns>
        Task<ICatalogue> LoadCatalogueAsync(CatalogueInfo info, LoadStreamedCatalogueCallback loadCallback);

        /// <summary>
        ///     Invokes provided callback with opened stream described by
        ///     <paramref name="info" /> .
        /// </summary>
        /// <param name="info">Identifies the game system to be opened.</param>
        /// <param name="loadCallback">Loads game system from provided stream.</param>
        /// <returns>Result of <paramref name="loadCallback" /> .</returns>
        Task<IGameSystem> LoadGameSystemAsync(GameSystemInfo info, LoadStreamedGameSystemCallback loadCallback);

        /// <summary>
        ///     Invokes provided callback with opened stream described by <paramref name="info" /> .
        /// </summary>
        /// <param name="info">Identifying the roster to be opened.</param>
        /// <param name="loadCallback">Loads roster from provided stream.</param>
        /// <returns>Result of <paramref name="loadCallback" /> .</returns>
        Task<IRoster> LoadRosterAsync(RosterInfo info, LoadStreamedRosterCallback loadCallback);

        /// <summary>
        ///     Invokes and awaits provided callback with opened stream described by
        ///     <paramref name="info" /> .
        /// </summary>
        /// <param name="info">Identifying the roster to be opened.</param>
        /// <param name="loadAsyncCallback">Loads roster from provided stream.</param>
        /// <returns>Result of <paramref name="loadAsyncCallback" /> .</returns>
        Task<IRoster> LoadRosterAsync(RosterInfo info, LoadStreamedRosterAsyncCallback loadAsyncCallback);

        /// <summary>
        ///     Saves catalogue written to provided stream under <paramref name="filename" /> or if null
        ///     or empty, under previous name.
        /// </summary>
        /// <param name="info">Describes saved catalogue.</param>
        /// <param name="filename">
        ///     Name of the file in which the catalogue should be saved. If null or empty, previous name
        ///     will be used.
        /// </param>
        /// <returns></returns>
        /// <exception cref="ArgumentException">
        ///     When <paramref name="filename" /> is null or empty and no previous is available (because
        ///     ie. this catalogue is new).
        /// </exception>
        /// <returns>Stream to write serialized catalogue into.</returns>
        Task<Stream> GetCatalogueOutputStreamAsync(CatalogueInfo info, string filename = null);

        /// <summary>
        ///     Saves game system written to provided stream under <paramref name="filename" /> or if
        ///     null or empty, under previous name.
        /// </summary>
        /// <param name="info">Describes saved game system.</param>
        /// <param name="filename">
        ///     Name of the file in which the catalogue should be saved. If null or empty, previous name
        ///     will be used.
        /// </param>
        /// <returns></returns>
        /// <exception cref="ArgumentException">
        ///     When <paramref name="filename" /> is null or empty and no previous is available (because
        ///     ie. this game system is new).
        /// </exception>
        /// <returns>Stream to write serialized game system into.</returns>
        Task<Stream> GetGameSystemOutputStreamAsync(GameSystemInfo info, string filename = null);

        /// <summary>
        ///     Saves the roster written to provided stream under <paramref name="filename" />.
        ///     Overwrites any previous file which has the same raw ID.
        /// </summary>
        /// <param name="info">Info of roster to be saved.</param>
        /// <param name="filename">
        ///     Name of the file in which the catalogue should be saved. If null or empty, previous name
        ///     will be used. If no previous name is available, some default or random name will be used.
        /// </param>
        /// <returns>Stream to write serialized roster into.</returns>
        Task<Stream> GetRosterOutputStreamAsync(RosterInfo info, string filename = null);
    }
}
