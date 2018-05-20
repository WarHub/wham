using Microsoft.XmlDiffPatch;
using System;
using System.IO;
using System.Xml;
using Xunit;

namespace WarHub.ArmouryModel.Source.BattleScribe.Tests
{
    public class BattleScribeFileTests : SerializationTestBase
    {
        [Fact]
        [Trait("XmlSerialization", "ReadWriteTest")]
        public void ReadWriteGamesystem()
        {
            ReadWriteXml(XmlTestData.GamesystemFilename, s => s.DeserializeGamesystem());
        }

        [Theory]
        [Trait("XmlSerialization", "ReadWriteTest")]
        [InlineData(XmlTestData.Catalogue1Filename)]
        [InlineData(XmlTestData.Catalogue2Filename)]
        public void ReadWriteCatalogue(string filename)
        {
            ReadWriteXml(filename, s => s.DeserializeCatalogue());
        }

        [Fact]
        [Trait("XmlSerialization", "ReadWriteTest")]
        public void ReadWriteRoster()
        {
            ReadWriteXml(XmlTestData.RosterFilename, s => s.DeserializeRoster());
        }

        private static void ReadWriteXml(string filename, Func<Stream, SourceNode> deserialize)
        {
            var input = Path.Combine(XmlTestData.InputDir, filename);
            var output = Path.Combine(XmlTestData.OutputDir, filename);
            var readNode = Deserialize();
            Serialize(readNode);
            var areXmlEqual = AreXmlEqual();
            Assert.True(areXmlEqual);
            
            SourceNode Deserialize()
            {
                using (var stream = File.OpenRead(input))
                {
                    return deserialize(stream);
                }
            }
            void Serialize(SourceNode node)
            {
                Assert.True(Directory.Exists(XmlTestData.OutputDir));
                using (var stream = File.Create(output))
                {
                    node.Serialize(stream);
                }
            }
            bool AreXmlEqual()
            {
                var differ = new XmlDiff(XmlDiffOptions.None);
                //using (var diffStream = new MemoryStream())
                using (var diffStream = File.Create(output + ".diff"))
                {
                    using (var diffWriter = XmlWriter.Create(diffStream))
                    {
                        var areEqual = differ.Compare(input, output, false, diffWriter);
                        return areEqual;
                    }
                }
            }
        }
    }
}
