// WarHub licenses this file to you under the MIT license.
// See LICENSE file in the project root for more information.

namespace WarHub.Armoury.Model.BattleScribe.Files
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.IO.Compression;
    using System.Linq;
    using System.Threading.Tasks;
    using Repo;

    /// <summary>
    ///     Provides methods to move BattleScribe catalogues from streams into <see cref="IRepoStorageService" />.
    /// </summary>
    [SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable",
        Justification = "No (IDisposable) instance field - shows erroneously.")]
    public class CatalogueFile
    {
        /// <summary>
        ///     Moves content of <paramref name="catalogueStream" /> into
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
        /// <param name="catalogueStream">Catalogue content.</param>
        /// <param name="filename">Desired catalogue filename.</param>
        /// <param name="repoStorageService">Saves the stream.</param>
        /// <returns>Info of moved catalogue.</returns>
        /// <exception cref="NotSupportedException">
        ///     When <paramref name="filename" /> has unsupported extension.
        /// </exception>
        public static async Task<CatalogueInfo> MoveToRepoStorageAsync(Stream catalogueStream, string filename,
            IRepoStorageService repoStorageService)
        {
            if (filename.EndsWith(".catz") || filename.EndsWith(".zip"))
            {
                return await MoveZippedToRepoStorageAsync(catalogueStream, filename, repoStorageService);
            }
            if (filename.EndsWith(".cat"))
            {
                return await MoveUnzippedToRepoStorageAsync(catalogueStream, filename, repoStorageService);
            }
            throw new NotSupportedException($"Cannot save catalogue '{filename}' - format is not supported.");
        }

        /// <summary>
        ///     Moves content of <paramref name="catalogueStream" /> into <paramref name="repoStorageService" />.
        /// </summary>
        /// <param name="catalogueStream">Catalogue content.</param>
        /// <param name="filename">Desired catalogue filename.</param>
        /// <param name="repoStorageService">Saves the stream.</param>
        /// <returns>Info of moved catalogue.</returns>
        public static async Task<CatalogueInfo> MoveUnzippedToRepoStorageAsync(Stream catalogueStream, string filename,
            IRepoStorageService repoStorageService)
        {
            if (catalogueStream.CanSeek)
            {
                return await MoveCoreAsync(catalogueStream, filename, repoStorageService);
            }
            using (var memoryStream = new MemoryStream())
            {
                catalogueStream.CopyTo(memoryStream);
                memoryStream.Seek(0, SeekOrigin.Begin);
                return await MoveCoreAsync(memoryStream, filename, repoStorageService);
            }
        }

        private static async Task<CatalogueInfo> MoveCoreAsync(Stream stream, string filename,
            IRepoStorageService repoStorageService)
        {
            var info = CatalogueInfo.CreateFromStream(stream);
            stream.Seek(0, SeekOrigin.Begin);
            using (var output = await repoStorageService.GetCatalogueOutputStreamAsync(info, filename))
            {
                stream.CopyTo(output);
            }
            return info;
        }

        /// <summary>
        ///     Unzips and moves content of <paramref name="catalogueStream" /> into
        ///     <paramref
        ///         name="repoStorageService" />
        ///     using name of zip entry as desired filename.
        /// </summary>
        /// <param name="catalogueStream">Catalogue content.</param>
        /// <param name="filename">Desired catalogue filename.</param>
        /// <param name="repoStorageService">Saves the stream.</param>
        /// <returns>Info of moved catalogue.</returns>
        /// <exception cref="NotSupportedException">
        ///     When zip archive contains more or less than exactly 1 entry.
        /// </exception>
        public static async Task<CatalogueInfo> MoveZippedToRepoStorageAsync(Stream catalogueStream, string filename,
            IRepoStorageService repoStorageService)
        {
            using (var archive = new ZipArchive(catalogueStream))
            {
                if (archive.Entries.Count != 1)
                {
                    throw new NotSupportedException(
                        $"Illegal zip archive entry count: {archive.Entries.Count} in catalogue '{filename}'");
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
