// WarHub licenses this file to you under the MIT license.
// See LICENSE file in the project root for more information.

namespace WarHub.Armoury.Model.DataAccess.ServiceImplementations
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using PCLStorage;
    using Repo;

    /// <summary>
    ///     Storage service using folder tree like so:
    ///     <code>
    /// * Data
    ///   * Rosters
    ///     * asdf-as-aasf-af (roster id)
    ///       * Roster.ros
    ///     * wefew-wef-wefwef (roster id)
    ///       * My Other Roster.ros
    ///   * Repos
    ///     * GUID1
    ///       * sample_game_system.gst
    ///       * sample_catalogue.cat
    ///     * GUID2 ...etc.
    /// </code>
    /// </summary>
    public class StorageService : IStorageService, IRepoStorageService
    {
        public const string DataFolderName = "Data";
        public const string ReposFolderName = "Repos";
        public const string RosterFileName = "Roster.ros";
        public const string RostersFolderName = "Rosters";

        protected Dictionary<string, GameSystemFilesIndex> RepoIndexes { get; }
            = new Dictionary<string, GameSystemFilesIndex>();

        public event EventHandler<NotifyRepoChangedEventArgs> RepoChanged;

        public async Task DeleteCatalogueAsync(CatalogueInfo catalogueInfo)
        {
            var catalogueFile = await GetReadableCatalogueFileAsync(catalogueInfo);
            await catalogueFile.DeleteAsync();
            RepoIndexes[catalogueInfo.GameSystemRawId].Remove(catalogueInfo.RawId);
            RaiseRepoChanged(catalogueInfo, RepoChange.Removal);
        }

        public async Task DeleteGameSystemAsync(GameSystemInfo gameSystemInfo)
        {
            var gameSystemFolder = await GetGameSystemFolderAsync(gameSystemInfo.RawId);
            await gameSystemFolder.DeleteAsync();
            RepoIndexes.Remove(gameSystemInfo.RawId);
            RaiseRepoChanged(gameSystemInfo, RepoChange.Removal);
        }

        public virtual async Task<ICatalogue> LoadCatalogueAsync(CatalogueInfo catalogueInfo,
            LoadStreamedCatalogueCallback loadCatalogueFromStream)
        {
            var catFile = await GetReadableCatalogueFileAsync(catalogueInfo);
            using (var stream = await catFile.OpenAsync(FileAccess.Read))
            {
                return loadCatalogueFromStream(stream);
            }
        }

        public virtual async Task<IGameSystem> LoadGameSystemAsync(GameSystemInfo gameSystemInfo,
            LoadStreamedGameSystemCallback loadGameSystemFromStream)
        {
            var gstFile = await GetReadableGameSystemFileAsync(gameSystemInfo);
            using (var stream = await gstFile.OpenAsync(FileAccess.Read))
            {
                return loadGameSystemFromStream(stream);
            }
        }

        public virtual async Task<IRoster> LoadRosterAsync(RosterInfo rosterInfo,
            LoadStreamedRosterCallback loadRosterFromStream)
        {
            var file = await GetReadableRosterFileAsync(rosterInfo);
            using (var stream = await file.OpenAsync(FileAccess.Read))
            {
                return loadRosterFromStream(stream);
            }
        }

        public virtual async Task<IRoster> LoadRosterAsync(RosterInfo rosterInfo,
            LoadStreamedRosterAsyncCallback loadRosterFromStream)
        {
            var file = await GetReadableRosterFileAsync(rosterInfo);
            using (var stream = await file.OpenAsync(FileAccess.Read))
            {
                return await loadRosterFromStream(stream);
            }
        }

        public virtual async Task<Stream> GetCatalogueOutputStreamAsync(CatalogueInfo catalogueInfo,
            string filename = null)
        {
            var fileAccessInfo = await GetWriteableCatalogueFileAsync(catalogueInfo, filename);
            return new CallbackOnDisposeStreamWrapper(await fileAccessInfo.File.OpenAsync(FileAccess.ReadAndWrite),
                () =>
                    RaiseRepoChanged(catalogueInfo,
                        fileAccessInfo.IsNewlyCreated ? RepoChange.Addition : RepoChange.Update));
        }

        public virtual async Task<Stream> GetGameSystemOutputStreamAsync(GameSystemInfo systemInfo,
            string filename = null)
        {
            var fileAccessInfo = await GetWriteableGameSystemFileAsync(systemInfo, filename);
            return new CallbackOnDisposeStreamWrapper(await fileAccessInfo.File.OpenAsync(FileAccess.ReadAndWrite),
                () =>
                    RaiseRepoChanged(systemInfo, fileAccessInfo.IsNewlyCreated ? RepoChange.Addition : RepoChange.Update));
        }

        public virtual async Task<Stream> GetRosterOutputStreamAsync(RosterInfo rosterInfo, string filename = null)
        {
            var fileAccessInfo = await GetWriteableRosterFileAsync(rosterInfo, filename);
            return new CallbackOnDisposeStreamWrapper(await fileAccessInfo.File.OpenAsync(FileAccess.ReadAndWrite),
                () =>
                    RaiseRepoChanged(rosterInfo, fileAccessInfo.IsNewlyCreated ? RepoChange.Addition : RepoChange.Update));
        }

        public async Task DeleteRosterAsync(RosterInfo rosterInfo)
        {
            var rostersFolder = await GetRostersFolderAsync();
            var folder = await rostersFolder.GetFolderAsync(rosterInfo.RawId);
            await folder.DeleteAsync();
            RaiseRepoChanged(rosterInfo, RepoChange.Removal);
        }

        public virtual async Task<IEnumerable<IFolder>> GetGameSystemFoldersAsync()
        {
            var repoFolder = await GetDataRepoFolderAsync();
            var systemFolders = await repoFolder.GetFoldersAsync();
            return systemFolders.ToList();
        }

        public async Task<IFile> GetReadableCatalogueFileAsync(CatalogueInfo info)
        {
            GameSystemFilesIndex systemIndex;
            if (RepoIndexes.TryGetValue(info.GameSystemRawId, out systemIndex) == false)
            {
                systemIndex = await IndexSystemAsync(info.GameSystemRawId);
            }
            return systemIndex[info.RawId];
        }

        public async Task<IFile> GetReadableGameSystemFileAsync(GameSystemInfo info)
        {
            GameSystemFilesIndex systemIndex;
            if (RepoIndexes.TryGetValue(info.RawId, out systemIndex) == false)
            {
                systemIndex = await IndexSystemAsync(info.RawId);
            }
            return systemIndex.GameSystemFile;
        }

        public async Task<IFile> GetReadableRosterFileAsync(RosterInfo info)
        {
            var rostersFolder = await GetRostersFolderAsync();
            var folder = await rostersFolder.CreateFolderAsync(info.RawId, CreationCollisionOption.OpenIfExists);
            var files = await folder.GetFilesAsync();
            if (files.Count != 1)
            {
                throw new StorageException(
                    $"{files.Count}) file{(files.Count != 1 ? "s" : string.Empty)} in roster folder." +
                    " Expected exactly one.");
            }
            var file = files.Single();
            return file;
        }

        public virtual async Task<IFolder> GetRostersFolderAsync()
        {
            var rootIFolder = await GetRootIFolderAsync();
            var rostersFolder =
                await rootIFolder.CreateFolderAsync(RostersFolderName, CreationCollisionOption.OpenIfExists);
            return rostersFolder;
        }

        protected virtual async Task<IFolder> GetDataRepoFolderAsync()
        {
            var rootFolder = await GetRootIFolderAsync();
            var dataFolder = await rootFolder.CreateFolderAsync(ReposFolderName, CreationCollisionOption.OpenIfExists);
            return dataFolder;
        }

        protected virtual async Task<IFolder> GetGameSystemFolderAsync(string gameSystemRawId)
        {
            var repoRootFolder = await GetDataRepoFolderAsync();
            var systemFolder =
                await repoRootFolder.CreateFolderAsync(gameSystemRawId, CreationCollisionOption.OpenIfExists);
            return systemFolder;
        }

        protected virtual async Task<IFolder> GetRootIFolderAsync()
        {
            var localFolder = FileSystem.Current.LocalStorage;
            return await localFolder.CreateFolderAsync(DataFolderName, CreationCollisionOption.OpenIfExists);
        }

        protected async Task<WriteableIFileAccessInfo> GetWriteableCatalogueFileAsync(CatalogueInfo info,
            string filename = null)
        {
            var isNewlyCreated = false;
            GameSystemFilesIndex index;
            if (RepoIndexes.TryGetValue(info.GameSystemRawId, out index) == false)
            {
                try
                {
                    index = await IndexSystemAsync(info.GameSystemRawId);
                }
                catch (StorageException e)
                {
                    throw new StorageException(
                        $"Cannot save '{info.Name}' (required game system id = '{info.GameSystemRawId}'): {e.Message}",
                        e);
                }
            }
            IFile file;
            if (index.TryGetValue(info.RawId, out file))
            {
                // exisitng file
                if (string.IsNullOrWhiteSpace(filename) == false && filename != file.Name)
                {
                    await file.RenameAsync(filename);
                }
            }
            else
            {
                // new file
                if (string.IsNullOrWhiteSpace(filename))
                {
                    throw new ArgumentException(
                        $"Cannot save new catalogue {info.Name} with {nameof(filename)}='{filename}'.");
                }
                var systemFolder = await GetGameSystemFolderAsync(info.GameSystemRawId);
                file = await systemFolder.CreateFileAsync(filename, CreationCollisionOption.ReplaceExisting);
                index[info.RawId] = file;
                isNewlyCreated = true;
            }
            return new WriteableIFileAccessInfo(file, isNewlyCreated);
        }

        protected async Task<WriteableIFileAccessInfo> GetWriteableGameSystemFileAsync(GameSystemInfo info,
            string filename = null)
        {
            var isNewlyCreated = false;
            GameSystemFilesIndex index;
            if (!RepoIndexes.TryGetValue(info.RawId, out index))
            {
                try
                {
                    index = await IndexSystemAsync(info.RawId);
                }
                catch (StorageException) // invalid number of game systems
                {
                    // create new game system
                    if (string.IsNullOrWhiteSpace(filename))
                    {
                        throw new ArgumentException(
                            $"Cannot save new game system '{info.Name}' with {nameof(filename)}='{filename}'");
                    }
                    var systemFolder = await GetGameSystemFolderAsync(info.RawId);
                    var systemFile =
                        await systemFolder.CreateFileAsync(filename, CreationCollisionOption.ReplaceExisting);
                    index = RepoIndexes[info.RawId] = new GameSystemFilesIndex(systemFile);
                    isNewlyCreated = true;
                }
            }
            var file = index.GameSystemFile;
            if (!string.IsNullOrWhiteSpace(filename) && filename != file.Name)
            {
                await file.RenameAsync(filename);
            }
            return new WriteableIFileAccessInfo(file, isNewlyCreated);
        }

        protected async Task<WriteableIFileAccessInfo> GetWriteableRosterFileAsync(RosterInfo info,
            string filename = null)
        {
            var rostersFolder = await GetRostersFolderAsync();
            var folder = await rostersFolder.CreateFolderAsync(info.RawId, CreationCollisionOption.ReplaceExisting);
            var desiredFilename = string.IsNullOrWhiteSpace(filename) ? RosterFileName : filename;
            var file = await folder.CreateFileAsync(desiredFilename, CreationCollisionOption.ReplaceExisting);
            return new WriteableIFileAccessInfo(file, false);
        }

        protected virtual async Task<GameSystemFilesIndex> IndexSystemAsync(string gameSystemRawId)
        {
            var systemFolder = await GetGameSystemFolderAsync(gameSystemRawId);
            var gameSystemTuple = await StorageIndexer.IndexGameSystemInfoAsync(systemFolder);
            var catalogueTuples = await StorageIndexer.IndexCatalogueInfosAsync(systemFolder);
            return RepoIndexes[gameSystemRawId] = new GameSystemFilesIndex(gameSystemTuple.File, catalogueTuples);
        }

        protected void RaiseRepoChanged(RosterInfo info, RepoChange changeType = RepoChange.Update)
        {
            RepoChanged?.Invoke(this, new NotifyRepoChangedEventArgs(info, changeType));
        }

        protected void RaiseRepoChanged(GameSystemInfo info, RepoChange changeType = RepoChange.Update)
        {
            RepoChanged?.Invoke(this, new NotifyRepoChangedEventArgs(info, changeType));
        }

        protected void RaiseRepoChanged(CatalogueInfo info, RepoChange changeType = RepoChange.Update)
        {
            RepoChanged?.Invoke(this, new NotifyRepoChangedEventArgs(info, changeType));
        }

        protected class GameSystemFilesIndex : Dictionary<string, IFile>
        {
            public GameSystemFilesIndex(IFile systemFile)
            {
                GameSystemFile = systemFile;
            }

            public GameSystemFilesIndex(IFile gameSystemFile, IList<StorageIndexer.CatalogueTuple> catalogueTuples)
            {
                GameSystemFile = gameSystemFile;
                foreach (var tuple in catalogueTuples)
                {
                    this[tuple.Info.RawId] = tuple.File;
                }
            }

            public IFile GameSystemFile { get; }
        }

        protected class WriteableIFileAccessInfo
        {
            public WriteableIFileAccessInfo(IFile file, bool isNewlyCreated)
            {
                File = file;
                IsNewlyCreated = isNewlyCreated;
            }

            public IFile File { get; }

            public bool IsNewlyCreated { get; }
        }

        protected class CallbackOnDisposeStreamWrapper : Stream
        {
            public CallbackOnDisposeStreamWrapper(Stream @base, Action disposalCallback = null)
            {
                if (@base == null)
                    throw new ArgumentNullException(nameof(@base));
                Base = @base;
                DisposalCallback = disposalCallback;
            }

            /// <summary>
            ///     When overridden in a derived class, gets a value indicating whether the current stream supports reading.
            /// </summary>
            /// <returns>
            ///     true if the stream supports reading; otherwise, false.
            /// </returns>
            public override bool CanRead => Base.CanRead;

            /// <summary>
            ///     When overridden in a derived class, gets a value indicating whether the current stream supports seeking.
            /// </summary>
            /// <returns>
            ///     true if the stream supports seeking; otherwise, false.
            /// </returns>
            public override bool CanSeek => Base.CanSeek;

            /// <summary>
            ///     When overridden in a derived class, gets a value indicating whether the current stream supports writing.
            /// </summary>
            /// <returns>
            ///     true if the stream supports writing; otherwise, false.
            /// </returns>
            public override bool CanWrite => Base.CanWrite;

            /// <summary>
            ///     When overridden in a derived class, gets the length in bytes of the stream.
            /// </summary>
            /// <returns>
            ///     A long value representing the length of the stream in bytes.
            /// </returns>
            /// <exception cref="T:System.NotSupportedException">A class derived from Stream does not support seeking. </exception>
            /// <exception cref="T:System.ObjectDisposedException">Methods were called after the stream was closed. </exception>
            public override long Length => Base.Length;

            /// <summary>
            ///     When overridden in a derived class, gets or sets the position within the current stream.
            /// </summary>
            /// <returns>
            ///     The current position within the stream.
            /// </returns>
            /// <exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
            /// <exception cref="T:System.NotSupportedException">The stream does not support seeking. </exception>
            /// <exception cref="T:System.ObjectDisposedException">Methods were called after the stream was closed. </exception>
            public override long Position
            {
                get { return Base.Position; }
                set { Base.Position = value; }
            }

            private Stream Base { get; }

            private Action DisposalCallback { get; }

            /// <summary>
            ///     Releases the unmanaged resources used by the <see cref="T:System.IO.Stream" /> and optionally releases the managed
            ///     resources.
            /// </summary>
            /// <param name="disposing">
            ///     true to release both managed and unmanaged resources; false to release only unmanaged
            ///     resources.
            /// </param>
            protected override void Dispose(bool disposing)
            {
                if (disposing)
                {
                    DisposalCallback?.Invoke();
                }
                Base.Dispose();
            }

            /// <summary>
            ///     When overridden in a derived class, clears all buffers for this stream and causes any buffered data to be written
            ///     to the underlying device.
            /// </summary>
            /// <exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
            public override void Flush()
            {
                Base.Flush();
            }

            /// <summary>
            ///     When overridden in a derived class, reads a sequence of bytes from the current stream and advances the position
            ///     within the stream by the number of bytes read.
            /// </summary>
            /// <returns>
            ///     The total number of bytes read into the buffer. This can be less than the number of bytes requested if that many
            ///     bytes are not currently available, or zero (0) if the end of the stream has been reached.
            /// </returns>
            /// <param name="buffer">
            ///     An array of bytes. When this method returns, the buffer contains the specified byte array with the
            ///     values between <paramref name="offset" /> and (<paramref name="offset" /> + <paramref name="count" /> - 1) replaced
            ///     by the bytes read from the current source.
            /// </param>
            /// <param name="offset">
            ///     The zero-based byte offset in <paramref name="buffer" /> at which to begin storing the data read
            ///     from the current stream.
            /// </param>
            /// <param name="count">The maximum number of bytes to be read from the current stream. </param>
            /// <exception cref="T:System.ArgumentException">
            ///     The sum of <paramref name="offset" /> and <paramref name="count" /> is
            ///     larger than the buffer length.
            /// </exception>
            /// <exception cref="T:System.ArgumentNullException"><paramref name="buffer" /> is null. </exception>
            /// <exception cref="T:System.ArgumentOutOfRangeException">
            ///     <paramref name="offset" /> or <paramref name="count" /> is
            ///     negative.
            /// </exception>
            /// <exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
            /// <exception cref="T:System.NotSupportedException">The stream does not support reading. </exception>
            /// <exception cref="T:System.ObjectDisposedException">Methods were called after the stream was closed. </exception>
            public override int Read(byte[] buffer, int offset, int count)
            {
                return Base.Read(buffer, offset, count);
            }

            /// <summary>
            ///     When overridden in a derived class, sets the position within the current stream.
            /// </summary>
            /// <returns>
            ///     The new position within the current stream.
            /// </returns>
            /// <param name="offset">A byte offset relative to the <paramref name="origin" /> parameter. </param>
            /// <param name="origin">
            ///     A value of type <see cref="T:System.IO.SeekOrigin" /> indicating the reference point used to
            ///     obtain the new position.
            /// </param>
            /// <exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
            /// <exception cref="T:System.NotSupportedException">
            ///     The stream does not support seeking, such as if the stream is
            ///     constructed from a pipe or console output.
            /// </exception>
            /// <exception cref="T:System.ObjectDisposedException">Methods were called after the stream was closed. </exception>
            public override long Seek(long offset, SeekOrigin origin)
            {
                return Base.Seek(offset, origin);
            }

            /// <summary>
            ///     When overridden in a derived class, sets the length of the current stream.
            /// </summary>
            /// <param name="value">The desired length of the current stream in bytes. </param>
            /// <exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
            /// <exception cref="T:System.NotSupportedException">
            ///     The stream does not support both writing and seeking, such as if the
            ///     stream is constructed from a pipe or console output.
            /// </exception>
            /// <exception cref="T:System.ObjectDisposedException">Methods were called after the stream was closed. </exception>
            public override void SetLength(long value)
            {
                Base.SetLength(value);
            }

            /// <summary>
            ///     When overridden in a derived class, writes a sequence of bytes to the current stream and advances the current
            ///     position within this stream by the number of bytes written.
            /// </summary>
            /// <param name="buffer">
            ///     An array of bytes. This method copies <paramref name="count" /> bytes from
            ///     <paramref name="buffer" /> to the current stream.
            /// </param>
            /// <param name="offset">
            ///     The zero-based byte offset in <paramref name="buffer" /> at which to begin copying bytes to the
            ///     current stream.
            /// </param>
            /// <param name="count">The number of bytes to be written to the current stream. </param>
            public override void Write(byte[] buffer, int offset, int count)
            {
                Base.Write(buffer, offset, count);
            }
        }
    }
}
