// WarHub licenses this file to you under the MIT license.
// See LICENSE file in the project root for more information.

namespace WarHub.Armoury.Model.BattleScribe.Files
{
    using System;
    using System.IO;
    using System.IO.Compression;
    using System.Linq;
    using System.Threading.Tasks;
    using Repo;

    /// <summary>
    ///     Provides methods to move BattleScribe game systems from streams into <see cref="IRepoStorageService" />.
    /// </summary>
    public class GameSystemFile
    {
        /// <summary>
        ///     Moves content of <paramref name="gameSystemStream" /> into
        ///     <paramref
        ///         name="repoStorageService" />
        ///     . Depending on <paramref name="filename" /> extension,
        ///     <see
        ///         cref="MoveZippedToRepoStorageAsync(Stream, string, IRepoStorageService)" />
        ///     or
        ///     <see
        ///         cref="MoveUnzippedToRepoStorageAsync(Stream, string, IRepoStorageService)" />
        ///     is invoked.
        /// </summary>
        /// <param name="gameSystemStream">Game system content.</param>
        /// <param name="filename">Desired game system filename.</param>
        /// <param name="repoStorageService">Saves the stream.</param>
        /// <returns>Info of moved game system.</returns>
        /// <exception cref="NotSupportedException">
        ///     When <paramref name="filename" /> has unsupported extension.
        /// </exception>
        public static async Task<GameSystemInfo> MoveToRepoStorageAsync(Stream gameSystemStream, string filename,
            IRepoStorageService repoStorageService)
        {
            if (filename.EndsWith(".gstz") || filename.EndsWith(".zip"))
            {
                return await MoveZippedToRepoStorageAsync(gameSystemStream, filename, repoStorageService);
            }
            if (filename.EndsWith(".gst"))
            {
                return await MoveUnzippedToRepoStorageAsync(gameSystemStream, filename, repoStorageService);
            }
            throw new NotSupportedException($"Cannot save game system '{filename}' - format is not supported.");
        }

        /// <summary>
        ///     Moves content of <paramref name="gameSystemStream" /> into <paramref name="repoStorageService" />.
        /// </summary>
        /// <param name="gameSystemStream">Game system content.</param>
        /// <param name="filename">Desired game system filename.</param>
        /// <param name="repoStorageService">Saves the stream.</param>
        /// <returns>Info of moved game system.</returns>
        public static async Task<GameSystemInfo> MoveUnzippedToRepoStorageAsync(Stream gameSystemStream, string filename,
            IRepoStorageService repoStorageService)
        {
            if (gameSystemStream.CanSeek)
            {
                return await MoveCoreAsync(gameSystemStream, filename, repoStorageService);
            }
            using (var memoryStream = new MemoryStream())
            {
                gameSystemStream.CopyTo(memoryStream);
                memoryStream.Seek(0, SeekOrigin.Begin);
                return await MoveCoreAsync(memoryStream, filename, repoStorageService);
            }
        }

        private static async Task<GameSystemInfo> MoveCoreAsync(Stream stream, string filename,
            IRepoStorageService repoStorageService)
        {
            var info = GameSystemInfo.CreateFromStream(stream);
            stream.Seek(0, SeekOrigin.Begin);
            using (var output = await repoStorageService.GetGameSystemOutputStreamAsync(info, filename))
            {
                stream.CopyTo(output);
            }
            return info;
        }

        /// <summary>
        ///     Unzips and moves content of <paramref name="gameSystemStream" /> into
        ///     <paramref
        ///         name="repoStorageService" />
        ///     using name of zip entry as desired filename.
        /// </summary>
        /// <param name="gameSystemStream">Game system content.</param>
        /// <param name="filename">Desired game system filename.</param>
        /// <param name="repoStorageService">Saves the stream.</param>
        /// <returns>Info of moved game system.</returns>
        /// <exception cref="NotSupportedException">
        ///     When zip archive contains more or less than exactly 1 entry.
        /// </exception>
        public static async Task<GameSystemInfo> MoveZippedToRepoStorageAsync(Stream gameSystemStream, string filename,
            IRepoStorageService repoStorageService)
        {
            using (var archive = new ZipArchive(gameSystemStream))
            {
                if (archive.Entries.Count != 1)
                {
                    throw new NotSupportedException(
                        $"Illegal zip archive entry count: {archive.Entries.Count} in game system '{filename}'");
                }
                var zipEntry = archive.Entries.Single();
                using (var unzippedStream = zipEntry.Open())
                {
                    return await MoveToRepoStorageAsync(unzippedStream, zipEntry.Name, repoStorageService);
                }
            }
        }
    }
}
