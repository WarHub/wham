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
        /// <param name="filepath">Filepath or filename to find out a format of the file.</param>
        /// <param name="stream">Contains index.xml content.</param>
        /// <returns>Created source index object.</returns>
        /// <exception cref="NotSupportedException">
        ///     If the <paramref name="filepath" /> has unsupported extension.
        /// </exception>
        /// <exception cref="InvalidDataException">When index could not be read.</exception>
        RemoteDataSourceIndex ReadIndexAuto(string filepath, Stream stream);

        /// <summary>
        ///     Moves content of <paramref name="stream" /> into <paramref name="repoStorageService" />.
        /// </summary>
        /// <param name="stream">Catalogue content.</param>
        /// <param name="filename">Desired catalogue filename.</param>
        /// <param name="repoStorageService">Saves the stream.</param>
        /// <returns>Info of moved catalogue.</returns>
        /// <exception cref="NotSupportedException">
        ///     When <paramref name="filename" /> has unsupported extension.
        /// </exception>
        Task<CatalogueInfo> MoveCatalogueToRepoStorageAsync(Stream stream, string filename,
            IRepoStorageService repoStorageService);

        /// <summary>
        ///     Moves content of <paramref name="stream" /> into <paramref name="repoStorageService" />.
        /// </summary>
        /// <param name="stream">Game system content.</param>
        /// <param name="filename">Desired game system filename.</param>
        /// <param name="repoStorageService">Saves the stream.</param>
        /// <returns>Info of moved game system.</returns>
        /// <exception cref="NotSupportedException">
        ///     When <paramref name="filename" /> has unsupported extension.
        /// </exception>
        Task<GameSystemInfo> MoveGameSystemToRepoStorageAsync(Stream stream, string filename,
            IRepoStorageService repoStorageService);

        /// <summary>
        ///     Moves content of <paramref name="stream" /> into <paramref name="repoStorageService" />.
        /// </summary>
        /// <param name="stream">Roster content.</param>
        /// <param name="filename">Desired roster filename.</param>
        /// <param name="repoStorageService">Saves the stream.</param>
        /// <returns>Info of moved roster.</returns>
        /// <exception cref="NotSupportedException">
        ///     When <paramref name="filename" /> has unsupported extension.
        /// </exception>
        Task<RosterInfo> MoveRosterToRepoStorageAsync(Stream stream, string filename,
            IRepoStorageService repoStorageService);
    }
}
