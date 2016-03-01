// WarHub licenses this file to you under the MIT license.
// See LICENSE file in the project root for more information.

namespace WarHub.Armoury.Model.BattleScribeXmlTests
{
    using System;
    using System.IO;
    using BattleScribeXml;
    using Xunit;

    public class XmlFactoryTests : IDisposable
    {
        public XmlFactoryTests()
        {
            CreateDir();
        }

        public void Dispose()
        {
            RemoveDir();
        }

        [Fact]
        public void CatalogueFormatAsBattleScribeTest()
        {
            FormatAsBattleScribeTestHelper<Catalogue>(TestData.CatalogueFilename);
        }

        [Fact]
        public void CatalogueReadWriteTest()
        {
            ReadWriteFactoryTestHelper<Catalogue>(TestData.CatalogueFilename);
        }

        private static void CreateDir()
        {
            Directory.CreateDirectory(TestData.OutputDir);
        }

        [Fact]
        public void DataIndexFormatAsBattleScribeTest()
        {
            FormatAsBattleScribeTestHelper<DataIndex>(TestData.IndexFilename);
        }

        [Fact]
        public void DataIndexReadWriteTest()
        {
            ReadWriteFactoryTestHelper<DataIndex>(TestData.IndexFilename);
        }

        [Fact]
        public void GameSystemFormatAsBattleScribeTest()
        {
            FormatAsBattleScribeTestHelper<GameSystem>(TestData.GameSystemFilename);
        }

        [Fact]
        public void GameSystemReadWriteTest()
        {
            ReadWriteFactoryTestHelper<GameSystem>(TestData.GameSystemFilename);
        }

        private static void RemoveDir()
        {
            Directory.Delete(TestData.OutputDir, true);
        }

        [Fact]
        public void RosterFormatAsBattleScribeTest()
        {
            FormatAsBattleScribeTestHelper<Roster>(TestData.RosterFilename);
        }

        [Fact]
        public void RosterReadWriteTest()
        {
            ReadWriteFactoryTestHelper<Roster>(TestData.RosterFilename);
        }

        private static void FormatAsBattleScribeTestHelper<T>(string filename)
            where T : IXmlProperties
        {
            var inputPath = TestData.InputDir + filename;
            T deserializedObject;
            using (Stream readStream = File.OpenRead(inputPath))
            {
                deserializedObject = XmlSerializer.Deserialize<T>(readStream);
            }
            Stream writeStream = new MemoryStream(); // disposed later by StreamReader
            XmlSerializer.SerializeFormatted(deserializedObject, writeStream);
            writeStream.Position = 0;
            //assertion
            using (var inputReader = new StreamReader(File.OpenRead(inputPath)))
            {
                using (var outputReader = new StreamReader(writeStream))
                {
                    Assert.False(outputReader.EndOfStream, "unexpected EOS");
                    ulong goodLines = 0;
                    while (!inputReader.EndOfStream && !outputReader.EndOfStream)
                    {
                        Assert.True(inputReader.ReadLine() == outputReader.ReadLine(),
                            $"line {goodLines + 1} wasn't same as original");
                        goodLines++;
                    }
                    //Assert.Inconclusive("Lines read to be ok: {0}", goodLines);
                    Assert.True(string.Empty == outputReader.ReadToEnd(), "output longer than expected");
                    Assert.True(outputReader.EndOfStream);
                    Assert.True(string.Empty == inputReader.ReadToEnd(), "intput longer than expected");
                    Assert.True(inputReader.EndOfStream);
                }
            }
        }

        private static void ReadWriteFactoryTestHelper<T>(string filename) where T : IXmlProperties
        {
            var inputPath = TestData.InputDir + filename;
            var outputPath = TestData.OutputDir + filename;
            T testObject;
            using (var readStream = File.OpenRead(inputPath))
            {
                testObject = XmlSerializer.Deserialize<T>(readStream);
            }
            Assert.True(Directory.Exists(TestData.OutputDir));
            using (var writeStream = File.Create(outputPath))
            {
                XmlSerializer.Serialize(testObject, writeStream);
            }
        }
    }
}
