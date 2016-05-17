// WarHub licenses this file to you under the MIT license.
// See LICENSE file in the project root for more information.

namespace WarHub.Armoury.Model.DataAccess
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using PCLStorage;
    using Repo;

    /// <summary>
    ///     Provides methods to access defined storage places. Responsible for file system access.
    /// </summary>
    public interface IStorageService
    {
        /// <summary>
        ///     Deletes specified roster permanently from storage.
        /// </summary>
        /// <param name="roster">Describes the roster to be removed.</param>
        /// <returns></returns>
        Task DeleteRosterAsync(RosterInfo roster);

        /// <summary>
        ///     Creates collection of all folders with data files.
        /// </summary>
        /// <returns>Created list.</returns>
        Task<IEnumerable<IFolder>> GetGameSystemFoldersAsync();

        /// <summary>
        ///     Finds described catalogue file in storage.
        /// </summary>
        /// <param name="info">Describes catalogue to be found.</param>
        /// <returns>Found catalogue file or null if file wasn't found.</returns>
        Task<IFile> GetReadableCatalogueFileAsync(CatalogueInfo info);

        /// <summary>
        ///     Finds described game system file in storage.
        /// </summary>
        /// <param name="info">Describes game system to be found.</param>
        /// <returns>Found game system file or null if file wasn't found.</returns>
        Task<IFile> GetReadableGameSystemFileAsync(GameSystemInfo info);

        /// <summary>
        ///     Finds described roster file in storage.
        /// </summary>
        /// <param name="info">Describes roster to be found.</param>
        /// <returns>Found roster file or null if file wasn't found.</returns>
        Task<IFile> GetReadableRosterFileAsync(RosterInfo info);

        /// <summary>
        ///     Provides access to root folder of roster storage structure.
        /// </summary>
        /// <returns>The root folder of roster storage.</returns>
        Task<IFolder> GetRostersFolderAsync();
    }
}
