using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Schema;
using FluentAssertions;
using Microsoft.XmlDiffPatch;
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

        [Theory]
        [InlineData(XmlTestData.GamesystemFilename, XmlInformation.RootElement.GameSystem)]
        [InlineData(XmlTestData.Catalogue1Filename, XmlInformation.RootElement.Catalogue)]
        [InlineData(XmlTestData.Catalogue2Filename, XmlInformation.RootElement.Catalogue)]
        [InlineData(XmlTestData.RosterFilename, XmlInformation.RootElement.Roster)]
        public void Validates_with_xsd(string filename, XmlInformation.RootElement rootElement)
        {
            var path = Path.Combine(XmlTestData.InputDir, filename);
            var validation = new List<ValidationEventArgs>();
            var schemaSet = ReadSchemaSet(rootElement, HandleValidation);
            var xmlSettings = new XmlReaderSettings
            {
                ValidationType = ValidationType.Schema,
                Schemas = schemaSet
            };
            xmlSettings.ValidationEventHandler += HandleValidation;

            using (var reader = XmlReader.Create(File.OpenRead(path), xmlSettings))
            {
                while (reader.Read()) { }
            }

            validation.Should().BeEmpty();

            void HandleValidation(object sender, ValidationEventArgs e) => validation.Add(e);
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

        private static XmlSchemaSet ReadSchemaSet(
            XmlInformation.RootElement rootElement,
            ValidationEventHandler validationEventHandler)
        {
            using (var xsdStream = XmlInformation.OpenXsdStream(rootElement))
            {
                var schema = XmlSchema.Read(xsdStream, validationEventHandler);
                var set = new XmlSchemaSet();
                set.ValidationEventHandler += validationEventHandler;
                set.Add(schema);
                set.Compile();
                return set;
            }
        }
    }
}
