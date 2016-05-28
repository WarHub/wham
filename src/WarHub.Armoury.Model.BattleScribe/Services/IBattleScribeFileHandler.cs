// WarHub licenses this file to you under the MIT license.
// See LICENSE file in the project root for more information.

namespace WarHub.Armoury.Model.BattleScribe.Services
{
    using System;
    using System.IO;
    using System.Threading.Tasks;
    using Repo;

    public interface IBattleScribeFileHandler
    {
        /// <summary>
        ///     Creates new source index based on BattleScribe index.xml or .bsi-formatted stream,
        ///     automatically choosing format depending on given filepath's file extension.
        /// </summary>
        /// <param name="stream">Contains index.xml content.</param>
        /// <param name="filepath">Filepath or filename to find out a format of the file.</param>
        /// <returns>Created source index object.</returns>
        /// <exception cref="NotSupportedException">
        ///     If the <paramref name="filepath" /> has unsupported extension.
        /// </exception>
        /// <exception cref="InvalidDataException">When index could not be read.</exception>
        RemoteSourceDataIndex ReadIndexAuto(Stream stream, string filepath);

        /// <summary>
        ///     Moves content of <paramref name="stream" /> into Repo Storage.
        /// </summary>
        /// <param name="stream">Catalogue content.</param>
        /// <param name="filename">Desired catalogue filename.</param>
        /// <returns>Info of moved catalogue.</returns>
        /// <exception cref="NotSupportedException">
        ///     When <paramref name="filename" /> has unsupported extension.
        /// </exception>
        Task<CatalogueInfo> MoveCatalogueToRepoStorageAsync(Stream stream, string filename);

        /// <summary>
        ///     Moves content of <paramref name="stream" /> into Repo Storage.
        /// </summary>
        /// <param name="stream">Game system content.</param>
        /// <param name="filename">Desired game system filename.</param>
        /// <returns>Info of moved game system.</returns>
        /// <exception cref="NotSupportedException">
        ///     When <paramref name="filename" /> has unsupported extension.
        /// </exception>
        Task<GameSystemInfo> MoveGameSystemToRepoStorageAsync(Stream stream, string filename);

        /// <summary>
        ///     Moves content of <paramref name="stream" /> into Repo Storage.
        /// </summary>
        /// <param name="stream">Roster content.</param>
        /// <param name="filename">Desired roster filename.</param>
        /// <returns>Info of moved roster.</returns>
        /// <exception cref="NotSupportedException">
        ///     When <paramref name="filename" /> has unsupported extension.
        /// </exception>
        Task<RosterInfo> MoveRosterToRepoStorageAsync(Stream stream, string filename);
    }
}
