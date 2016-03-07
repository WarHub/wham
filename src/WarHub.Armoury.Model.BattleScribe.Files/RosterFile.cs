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
    ///     Provides methods to move BattleScribe rosters from streams into <see cref="IRepoStorageService" />.
    /// </summary>
    public class RosterFile
    {
        /// <summary>
        ///     Moves content of <paramref name="rosterStream" /> into
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
        /// <param name="rosterStream">Roster content.</param>
        /// <param name="filename">Desired roster filename.</param>
        /// <param name="repoStorageService">Saves the stream.</param>
        /// <returns>Info of moved roster.</returns>
        /// <exception cref="NotSupportedException">
        ///     When <paramref name="filename" /> has unsupported extension.
        /// </exception>
        public static async Task<RosterInfo> MoveToRepoStorageAsync(Stream rosterStream, string filename,
            IRepoStorageService repoStorageService)
        {
            if (filename.EndsWith(".rosz") || filename.EndsWith(".zip"))
            {
                return await MoveZippedToRepoStorageAsync(rosterStream, filename, repoStorageService);
            }
            if (filename.EndsWith(".ros"))
            {
                return await MoveUnzippedToRepoStorageAsync(rosterStream, filename, repoStorageService);
            }
            throw new NotSupportedException($"Cannot save roster '{filename}' - format is not supported.");
        }

        /// <summary>
        ///     Moves content of <paramref name="rosterStream" /> into <paramref name="repoStorageService" />.
        /// </summary>
        /// <param name="rosterStream">Roster content.</param>
        /// <param name="filename">Desired roster filename.</param>
        /// <param name="repoStorageService">Saves the stream.</param>
        /// <returns>Info of moved roster.</returns>
        public static async Task<RosterInfo> MoveUnzippedToRepoStorageAsync(Stream rosterStream, string filename,
            IRepoStorageService repoStorageService)
        {
            if (rosterStream.CanSeek)
            {
                return await MoveCoreAsync(rosterStream, filename, repoStorageService);
            }
            using (var memoryStream = new MemoryStream())
            {
                rosterStream.CopyTo(memoryStream);
                memoryStream.Seek(0, SeekOrigin.Begin);
                return await MoveCoreAsync(memoryStream, filename, repoStorageService);
            }
        }

        private static async Task<RosterInfo> MoveCoreAsync(Stream stream, string filename,
            IRepoStorageService repoStorageService)
        {
            if (!stream.CanSeek)
                throw new ArgumentException("Cannot Seek.", nameof(stream));
            var info = RosterInfo.CreateFromStream(stream);
            stream.Seek(0, SeekOrigin.Begin);
            using (var output = await repoStorageService.GetRosterOutputStreamAsync(info, filename))
            {
                stream.CopyTo(output);
            }
            return info;
        }

        /// <summary>
        ///     Unzips and moves content of <paramref name="rosterStream" /> into
        ///     <paramref
        ///         name="repoStorageService" />
        ///     using name of zip entry as desired filename.
        /// </summary>
        /// <param name="rosterStream">Roster content.</param>
        /// <param name="filename">Desired roster filename.</param>
        /// <param name="repoStorageService">Saves the stream.</param>
        /// <returns>Info of moved roster.</returns>
        /// <exception cref="NotSupportedException">
        ///     When zip archive contains more or less than exactly 1 entry.
        /// </exception>
        public static async Task<RosterInfo> MoveZippedToRepoStorageAsync(Stream rosterStream, string filename,
            IRepoStorageService repoStorageService)
        {
            using (var archive = new ZipArchive(rosterStream))
            {
                if (archive.Entries.Count != 1)
                {
                    throw new NotSupportedException(
                        $"Illegal zip archive entry count: {archive.Entries.Count} in roster '{filename}'");
                }
                var zipEntry = archive.Entries.Single();
                using (var unzippedStream = zipEntry.Open())
                {
                    return await MoveToRepoStorageAsync(unzippedStream, zipEntry.Name, repoStorageService);
                }
            }
        }

        /// <summary>
        ///     Zips source into target stream, saving the source under <paramref name="zipEntryName" />.
        /// </summary>
        /// <param name="zipEntryName">Name of zip entry in created zip archive.</param>
        /// <param name="inputStream">Input that is compressed into created zip archive.</param>
        /// <param name="outputStream">Output stream into which the created archive is written.</param>
        public static void Zip(string zipEntryName, Stream inputStream, Stream outputStream)
        {
            using (var archive = new ZipArchive(outputStream, ZipArchiveMode.Create, true))
            {
                var entry = archive.CreateEntry(zipEntryName, CompressionLevel.Optimal);
                using (var entryStream = entry.Open())
                {
                    inputStream.CopyTo(entryStream);
                }
            }
        }
    }
}
