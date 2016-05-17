// WarHub licenses this file to you under the MIT license.
// See LICENSE file in the project root for more information.

namespace WarHub.Armoury.Model.DataAccess
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using PCLStorage;
    using Repo;

    /// <summary>
    ///     Reads Info from repo files and indexes folder contents. Describes file extensions.
    /// </summary>
    public static class StorageIndexer
    {
        public const string CatalogueExtension = ".cat";

        public const string GameSystemExtension = ".gst";

        public const string RosterExtension = ".ros";

        /// <summary>
        ///     Reads <see cref="CatalogueInfo" /> for each IFile with
        ///     <see
        ///         cref="CatalogueExtension" />
        ///     in given folder.
        /// </summary>
        /// <param name="repoFolder">Folder to search for catalogues in.</param>
        /// <returns>List of read catalogues' properties.</returns>
        public static async Task<IList<CatalogueTuple>> IndexCatalogueInfosAsync(IFolder repoFolder)
        {
            var repoFileList = await repoFolder.GetFilesAsync();
            var catalogueFiles = repoFileList.Where(IsCatalogue);
            var tuples = new List<CatalogueTuple>();
            foreach (var file in catalogueFiles)
            {
                var info = await ReadCatalogueInfoAsync(file);
                tuples.Add(new CatalogueTuple(file, info));
            }
            return tuples;
        }

        /// <summary>
        ///     Reads <see cref="GameSystemInfo" /> from given folder.
        /// </summary>
        /// <param name="repoFolder">Folder to search for game system files in.</param>
        /// <returns>Properties of the game system.</returns>
        /// <exception cref="StorageException">
        ///     When there is != 1 game system files in the folder.
        /// </exception>
        public static async Task<GameSystemTuple> IndexGameSystemInfoAsync(IFolder repoFolder)
        {
            var repoFileList = await repoFolder.GetFilesAsync();
            var gstCount = repoFileList.Count(IsGameSystem);
            if (gstCount != 1)
            {
                var message = $"Corrupted local '{repoFolder.Name}' data repo." +
                              $" Found {gstCount} game system file{(gstCount == 1 ? "" : "s")}." +
                              " Those data files were removed. You may have to download them again.";
                try
                {
                    await repoFolder.DeleteAsync();
                    throw new StorageException(message);
                }
                catch (Exception e) when (!(e is StorageException))
                {
                    //failure in deleting files is not critical
                    throw new StorageException(message, e);
                }
            }
            var file = repoFileList.First(IsGameSystem);
            var info = await ReadGameSystemInfoAsync(file);
            return new GameSystemTuple(file, info);
        }

        /// <summary>
        ///     Reads <see cref="RosterInfo" /> for each <see cref="IFile" /> with
        ///     <see
        ///         cref="RosterExtension" />
        ///     in given folder.
        /// </summary>
        /// <param name="rostersFolder">Folder to search for catalogue files in.</param>
        /// <param name="gstGuid">
        ///     Identifies game system for which catalogues should be indexed. If null or empty, all
        ///     saved rosters are indexed.
        /// </param>
        /// <returns>
        ///     Properites of catalogues for game system identified by <paramref name="gstGuid" /> .
        /// </returns>
        public static async Task<IList<RosterInfo>> IndexRosterInfosAsync(IFolder rostersFolder,
            string gstGuid = null)
        {
            var rosterFiles = await IndexRostersAsync(rostersFolder);
            var rosterInfos = new List<RosterInfo>();
            foreach (var rosterFile in rosterFiles)
            {
                var rosterInfo = await ReadRosterInfoAsync(rosterFile);
                rosterInfos.Add(rosterInfo);
            }
            if (string.IsNullOrEmpty(gstGuid))
            {
                return rosterInfos.ToList();
            }
            return rosterInfos
                .Where(rosterInfo => rosterInfo.GameSystemRawId.Equals(gstGuid))
                .ToList();
        }

        /// <summary>
        ///     Finds all rosters contained in given folder, according to the convention that each
        ///     roster is contained in its own folder. The folder's name is the id of the roster within.
        /// </summary>
        /// <param name="rostersFolder">Root folder of roster folders.</param>
        /// <returns>Collection of roster files.</returns>
        public static async Task<IEnumerable<IFile>> IndexRostersAsync(IFolder rostersFolder)
        {
            var folderList = await rostersFolder.GetFoldersAsync();
            var rosterFiles = new List<IFile>();
            foreach (var folder in folderList)
            {
                var files = await folder.GetFilesAsync();
                rosterFiles.Add(files.Single(IsRoster));
            }
            return rosterFiles.Where(IsRoster);
        }

        /// <summary>
        ///     Reads basic properties of the catalogue in provided file.
        /// </summary>
        /// <param name="catalogueFile">Contains catalogue to be indexed.</param>
        /// <returns>Properties of the catalogue.</returns>
        public static async Task<CatalogueInfo> ReadCatalogueInfoAsync(IFile catalogueFile)
        {
            using (var stream = await catalogueFile.OpenAsync(FileAccess.Read))
            {
                return CatalogueInfo.CreateFromStream(stream);
            }
        }

        /// <summary>
        ///     Reads basic properties of the game system in provided file.
        /// </summary>
        /// <param name="gameSystemFile">Contains game system to be indexed.</param>
        /// <returns>Properties of the game system.</returns>
        public static async Task<GameSystemInfo> ReadGameSystemInfoAsync(IFile gameSystemFile)
        {
            using (var stream = await gameSystemFile.OpenAsync(FileAccess.Read))
            {
                return GameSystemInfo.CreateFromStream(stream);
            }
        }

        /// <summary>
        ///     Reads basic properties of the roster in provided file.
        /// </summary>
        /// <param name="rosterFile">Contains roster to be indexed.</param>
        /// <returns>Properties of the roster.</returns>
        public static async Task<RosterInfo> ReadRosterInfoAsync(IFile rosterFile)
        {
            using (var stream = await rosterFile.OpenAsync(FileAccess.Read))
            {
                return RosterInfo.CreateFromStream(stream);
            }
        }

        /// <summary>
        ///     In foreach loop awaits for each transformation to finish.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="sourceEnum"></param>
        /// <param name="transformation"></param>
        /// <returns></returns>
        public static async Task<IEnumerable<TResult>> SelectAsync<T, TResult>(
            this IEnumerable<T> sourceEnum,
            Func<T, Task<TResult>> transformation)
        {
            var resultEnum = new List<TResult>();
            foreach (var item in sourceEnum)
            {
                resultEnum.Add(await transformation(item));
            }
            return resultEnum;
        }

        private static bool ExtensionEquals(string extension, IFile file)
        {
            return file.Name.EndsWith(extension);
        }

        private static bool IsCatalogue(IFile file)
        {
            return ExtensionEquals(CatalogueExtension, file);
        }

        private static bool IsGameSystem(IFile file)
        {
            return ExtensionEquals(GameSystemExtension, file);
        }

        private static bool IsRoster(IFile file)
        {
            return ExtensionEquals(RosterExtension, file);
        }

        public class CatalogueTuple : StorageInfoTuple<CatalogueInfo>
        {
            public CatalogueTuple(IFile file, CatalogueInfo info)
                : base(file, info)
            {
            }
        }

        public class GameSystemTuple : StorageInfoTuple<GameSystemInfo>
        {
            public GameSystemTuple(IFile file, GameSystemInfo info)
                : base(file, info)
            {
            }
        }

        public class StorageInfoTuple<TInfo>
        {
            public StorageInfoTuple(IFile file, TInfo info)
            {
                File = file;
                Info = info;
            }

            public IFile File { get; }

            public TInfo Info { get; }
        }
    }
}
