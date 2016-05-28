// WarHub licenses this file to you under the MIT license.
// See LICENSE file in the project root for more information.

namespace WarHub.Armoury.Model.DataAccess.Serialization
{
    using System.Threading.Tasks;
    using System.Xml.Serialization;
    using PCLStorage;

    public abstract class ItemStore<TSerializableItem> : IItemStore<TSerializableItem>
    {
        protected ItemStore(IFileSystem fileSystem)
        {
            FileSystem = fileSystem;
        }

        protected virtual string DataFolderName { get; } = "Data";

        protected abstract string FileName { get; }

        private IFileSystem FileSystem { get; }

        public async Task<TSerializableItem> LoadItemAsync()
        {
            var serializer = new XmlSerializer(typeof(TSerializableItem));
            TSerializableItem index;
            var indexFile = await GetFileToReadAsync();
            using (var stream = await indexFile.OpenAsync(FileAccess.Read))
            {
                index = (TSerializableItem) serializer.Deserialize(stream);
            }
            //try
            //{
            //    indexFile = await GetFileToReadAsync();
            //    using (var stream = await indexFile.OpenAsync(FileAccess.Read))
            //    {
            //        index = (TSerializableIndex)serializer.Deserialize(stream);
            //    }
            //}
            //catch (FileNotFoundException)
            //{
            //    // ignore, it's possibly fresh install
            //    index = null;
            //    App.TelemetryClient.TrackEvent($"{IndexFileName} FileNotFoundException");
            //}
            //catch (IOException e)
            //{
            //    // log, it's unexpected exception
            //    index = null;
            //    App.TelemetryClient.TrackException(e);
            //}
            //catch (InvalidOperationException e)
            //{
            //    // incorrect xml, log
            //    index = null;
            //    await TryLogIndexContent(e, indexFile);
            //}
            //catch (Exception e)
            //{
            //    // ignoring, index to be rebuilt
            //    index = null;
            //    App.TelemetryClient.TrackException(e);
            //}
            return index;
        }

        public async Task SaveItemAsync(TSerializableItem item)
        {
            var serializer = new XmlSerializer(typeof(TSerializableItem));
            var indexFile = await GetFileToWriteAsync();
            using (var stream = await indexFile.OpenAsync(FileAccess.ReadAndWrite))
            {
                serializer.Serialize(stream, item);
            }
            //try
            //{
            //    var indexFile = await GetFileToWriteAsync();
            //    using (var stream = await indexFile.OpenAsync(FileAccess.ReadAndWrite))
            //    {
            //        serializer.Serialize(stream, (TSerializableIndex)index);
            //    }
            //}
            //catch (Exception e)
            //{
            //    App.TelemetryClient.TrackException(e);
            //}
        }

        private async Task<IFolder> GetDataFolderAsync()
        {
            var localFolder = FileSystem.LocalStorage;
            return await localFolder.CreateFolderAsync(DataFolderName, CreationCollisionOption.OpenIfExists);
        }

        private async Task<IFile> GetFileToReadAsync()
        {
            var dataFolder = await GetDataFolderAsync();
            return await dataFolder.GetFileAsync(FileName);
        }

        private async Task<IFile> GetFileToWriteAsync()
        {
            var dataFolder = await GetDataFolderAsync();
            return await dataFolder.CreateFileAsync(FileName, CreationCollisionOption.ReplaceExisting);
        }
    }
}
