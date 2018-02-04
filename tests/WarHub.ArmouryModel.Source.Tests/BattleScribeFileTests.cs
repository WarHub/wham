using System;
using System.IO;
using Xunit;

namespace WarHub.ArmouryModel.Source.Tests
{
    public class BattleScribeFileTests : SerializationTestBase
    {
        [Fact]
        [Trait("XmlSerialization", "ReadWriteTest")]
        public void ReadWriteGameSystem()
        {
            ReadWriteXml(XmlTestData.GameSystemFilename, SerializeGameSystem, DeserializeGameSystem);
        }

        [Theory]
        [Trait("XmlSerialization", "ReadWriteTest")]
        [InlineData(XmlTestData.Catalogue1Filename)]
        [InlineData(XmlTestData.Catalogue2Filename)]
        public void ReadWriteCatalogue(string filename)
        {
            ReadWriteXml(filename, SerializeCatalogue, DeserializeCatalogue);
        }

        [Fact]
        [Trait("XmlSerialization", "ReadWriteTest")]
        public void ReadWriteRoster()
        {
            ReadWriteXml(XmlTestData.RosterFilename, SerializeRoster, DeserializeRoster);
        }

        private static void ReadWriteXml(string filename, Action<SourceNode, Stream> serialize, Func<Stream, SourceNode> deserialize)
        {
            var readNode = Deserialize();
            Serialize(readNode);
            SourceNode Deserialize()
            {
                var input = Path.Combine(XmlTestData.InputDir, filename);
                using (var stream = File.OpenRead(input))
                {
                    return deserialize(stream);
                }
            }
            void Serialize(SourceNode node)
            {
                Assert.True(Directory.Exists(XmlTestData.OutputDir));
                var output = Path.Combine(XmlTestData.OutputDir, filename);
                using (var stream = File.Create(output))
                {
                    serialize(node, stream);
                }
            }
        }
    }
}
