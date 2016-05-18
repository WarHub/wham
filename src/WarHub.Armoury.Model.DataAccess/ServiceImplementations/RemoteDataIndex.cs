// WarHub licenses this file to you under the MIT license.
// See LICENSE file in the project root for more information.

namespace WarHub.Armoury.Model.DataAccess.ServiceImplementations
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Xml.Serialization;
    using PCLStorage;
    using Serialization;

    public class RemoteDataIndex : IRemoteDataIndex
    {
        public const string DataFolderName = "Data";
        public const string IndexFileName = "RemoteDataIndex.xml";

        public RemoteDataIndex(IDispatcher dispatcher)
        {
            Dispatcher = dispatcher;
            Load();
        }

        private IDispatcher Dispatcher { get; }

        private ObservableCollection<RemoteDataSourceInfo> Entries { get; } =
            new ObservableCollection<RemoteDataSourceInfo>();

        IEnumerable<RemoteDataSourceInfo> IRemoteDataIndex.Entries => Entries;

        public void AddEntry(RemoteDataSourceInfo entry)
        {
            Entries.Add(entry);
            Save();
        }

        public void RemoveEntry(RemoteDataSourceInfo entry)
        {
            Entries.Remove(entry);
            Save();
        }

        private async void Save()
        {
            var index = new SerializableRemoteDataSourceIndex
            {
                DataSourceInfos = Entries.Select(x => (SerializableRemoteDataSourceInfo) x).ToList()
            };
            await SaveIndexAsync(index);
        }

        private async void Load()
        {
            var index = await LoadIndexAsync();
            if (index == null)
            {
                return;
            }
            await Dispatcher.InvokeOnUiAsync(() => UpdateEntries(index));
        }

        private void UpdateEntries(SerializableRemoteDataSourceIndex index)
        {
            Entries.Clear();
            foreach (var dataSourceInfo in index.DataSourceInfos)
            {
                Entries.Add(dataSourceInfo);
            }
        }

        private async Task<SerializableRemoteDataSourceIndex> LoadIndexAsync()
        {
            var serializer = new XmlSerializer(typeof(SerializableRemoteDataSourceIndex));
            SerializableRemoteDataSourceIndex index;
            IFile indexFile = null;
            try
            {
                indexFile = await GetFileToReadAsync();
                using (var stream = await indexFile.OpenAsync(FileAccess.Read))
                {
                    index = (SerializableRemoteDataSourceIndex) serializer.Deserialize(stream);
                }
            }
            catch (FileNotFoundException)
            {
                // ignore, it's possibly fresh install
                index = null;
                App.TelemetryClient.TrackEvent($"{IndexFileName} FileNotFoundException");
            }
            catch (IOException e)
            {
                // log, it's unexpected exception
                index = null;
                App.TelemetryClient.TrackException(e);
            }
            catch (InvalidOperationException e)
            {
                // incorrect xml, log
                index = null;
                await TryLogIndexContent(e, indexFile);
            }
            catch (Exception e)
            {
                // ignoring, index to be rebuilt
                index = null;
                App.TelemetryClient.TrackException(e);
            }
            return index;
        }

        private static async Task SaveIndexAsync(SerializableRemoteDataSourceIndex index)
        {
            var serializer = new XmlSerializer(typeof(SerializableRemoteDataSourceIndex));
            try
            {
                var indexFile = await GetFileToWriteAsync();
                using (var stream = await indexFile.OpenAsync(FileAccess.ReadAndWrite))
                {
                    serializer.Serialize(stream, index);
                }
            }
            catch (Exception e)
            {
                App.TelemetryClient.TrackException(e);
            }
        }

        private static async Task TryLogIndexContent(InvalidOperationException e, IFile indexFile)
        {
            try
            {
                string fileString;
                using (var stream = await indexFile.OpenAsync(FileAccess.Read))
                using (var reader = new StreamReader(stream))
                {
                    fileString = await reader.ReadToEndAsync();
                }
                App.TelemetryClient.TrackException(e,
                    new Dictionary<string, string> {["RemoteDataSourceIndex content"] = fileString});
            }
            catch (Exception ex)
            {
                // ignoring reporting failure
                App.TelemetryClient.TrackException(e);
                App.TelemetryClient.TrackException(ex);
            }
        }

        private static async Task<IFolder> GetDataFolderAsync()
        {
            var localFolder = FileSystem.Current.LocalStorage;
            return await localFolder.CreateFolderAsync(DataFolderName, CreationCollisionOption.OpenIfExists);
        }

        private static async Task<IFile> GetFileToReadAsync()
        {
            var dataFolder = await GetDataFolderAsync();
            return await dataFolder.GetFileAsync(IndexFileName);
        }

        private static async Task<IFile> GetFileToWriteAsync()
        {
            var dataFolder = await GetDataFolderAsync();
            return await dataFolder.CreateFileAsync(IndexFileName, CreationCollisionOption.ReplaceExisting);
        }
    }
}
