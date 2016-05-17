// WarHub licenses this file to you under the MIT license.
// See LICENSE file in the project root for more information.

namespace WarHub.Armoury.Model.DataAccess.ServiceImplementations
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading.Tasks;
    using System.Xml.Serialization;
    using PCLStorage;
    using Serialization;

    public class DataIndexAccessService : IDataIndexAccessService
    {
        public const string DataFolderName = "Data";
        public const string IndexFileName = "DataIndex.xml";

        public async Task<DataIndex> LoadIndexAsync()
        {
            var serializer = new XmlSerializer(typeof(SerializableDataIndex));
            SerializableDataIndex index;
            IFile indexFile = null;
            try
            {
                indexFile = await GetFileToReadAsync();
                using (var stream = await indexFile.OpenAsync(FileAccess.Read))
                {
                    index = (SerializableDataIndex) serializer.Deserialize(stream);
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

        public async Task SaveIndexAsync(DataIndex index)
        {
            var serializer = new XmlSerializer(typeof(SerializableDataIndex));
            try
            {
                var indexFile = await GetFileToWriteAsync();
                using (var stream = await indexFile.OpenAsync(FileAccess.ReadAndWrite))
                {
                    serializer.Serialize(stream, (SerializableDataIndex) index);
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
                    new Dictionary<string, string> {["DataIndex content"] = fileString});
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

        public static async Task<IFile> GetFileToReadAsync()
        {
            var dataFolder = await GetDataFolderAsync();
            return await dataFolder.GetFileAsync(IndexFileName);
        }

        public static async Task<IFile> GetFileToWriteAsync()
        {
            var dataFolder = await GetDataFolderAsync();
            return await dataFolder.CreateFileAsync(IndexFileName, CreationCollisionOption.ReplaceExisting);
        }
    }

    // TODO telemetry? logger?

    internal static class App
    {
        public static TelemetryClient TelemetryClient { get; } = new TelemetryClient();
    }

    internal class TelemetryClient
    {
// ReSharper disable MemberCanBeMadeStatic.Local
        public void TrackException(Exception e)
        {
        }

        public void TrackException(Exception e, IDictionary<string, string> properties)
        {
        }

        public void TrackEvent(string eventName)
        {
        }
    }
}
