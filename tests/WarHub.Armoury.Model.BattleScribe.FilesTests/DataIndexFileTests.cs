// WarHub licenses this file to you under the MIT license.
// See LICENSE file in the project root for more information.

namespace WarHub.Armoury.Model.BattleScribe.FilesTests
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using BattleScribeXml;
    using Files;
    using Repo;
    using Xunit;

    public class DataIndexFileTests
    {
        [Fact]
        public void DataIndexWithoutSourceUriFailTest()
        {
            var index = CreateSampleDataIndex();
            index.IndexUrl = null;
            index.RepositoryUrls = null;
            using (var memoryStream = new MemoryStream())
            {
                XmlSerializer.Serialize(index, memoryStream);
                memoryStream.Seek(0, SeekOrigin.Begin);

                Assert.Throws<InvalidDataException>(
                    () => { DataIndexFile.ReadBattleScribeIndexAuto("index.xml", memoryStream); });
            }
        }

        [Fact]
        public void DataIndexWrongExtensionFailTest()
        {
            var index = CreateSampleDataIndex();
            using (var memoryStream = new MemoryStream())
            {
                XmlSerializer.Serialize(index, memoryStream);
                memoryStream.Seek(0, SeekOrigin.Begin);

                Assert.Throws<NotSupportedException>(
                    () => { DataIndexFile.ReadBattleScribeIndexAuto("index.docx", memoryStream); });
            }
        }

        private static DataIndex CreateSampleDataIndex()
        {
            const string battleScribeVersion = "test program v1";
            var index = new DataIndex
            {
                BattleScribeVersion = battleScribeVersion,
                Name = "Test index",
                DataIndexEntries = new List<DataIndexEntry>
                {
                    new DataIndexEntry
                    {
                        DataBattleScribeVersion = battleScribeVersion,
                        DataName = "Some game system",
                        DataRawId = "aaa-bb-cc-ddd",
                        DataRevision = 12,
                        DataType = RemoteDataType.GameSystem,
                        FilePath = "some game system.gst"
                    },
                    new DataIndexEntry
                    {
                        DataBattleScribeVersion = battleScribeVersion,
                        DataName = "Some catalogue",
                        DataRawId = "zzz-yy-xx-vvv",
                        DataRevision = 321,
                        DataType = RemoteDataType.Catalogue,
                        FilePath = "some catalogue.cat"
                    }
                },
                IndexUrl = "http://example.com/battlescribeindex/sample/index.bsi",
                RepositoryUrls = new List<string>
                {
                    "http://example-mirror.com/index.bsi",
                    "http://example-mirror.uk/index.bsi"
                }
            };
            return index;
        }
    }
}
