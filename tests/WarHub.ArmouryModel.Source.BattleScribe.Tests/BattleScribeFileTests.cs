﻿using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Schema;
using FluentAssertions;
using Microsoft.XmlDiffPatch;
using WarHub.ArmouryModel.Source.XmlFormat;
using Xunit;

namespace WarHub.ArmouryModel.Source.BattleScribe.Tests
{
    public class BattleScribeFileTests
    {
        [Fact]
        [Trait("XmlSerialization", "ReadWriteTest")]
        public void ReadWriteGamesystem()
        {
            ReadWriteXml(TestData.Gamesystem);
        }

        [Fact]
        [Trait("XmlSerialization", "ReadWriteTest")]
        public void ReadWriteCatalogue()
        {
            ReadWriteXml(TestData.Catalogue);
        }

        [Fact]
        [Trait("XmlSerialization", "ReadWriteTest")]
        public void ReadWriteRoster()
        {
            ReadWriteXml(TestData.Roster);
        }

        [Theory]
        [InlineData(TestData.Gamesystem, RootElement.GameSystem)]
        [InlineData(TestData.Catalogue, RootElement.Catalogue)]
        [InlineData(TestData.Roster, RootElement.Roster)]
        public void Validates_with_xsd(string datafile, RootElement rootElement)
        {
            var validation = new List<ValidationEventArgs>();
            var schemaSet = ReadSchemaSet(rootElement, HandleValidation);
            var xmlSettings = new XmlReaderSettings
            {
                ValidationType = ValidationType.Schema,
                Schemas = schemaSet
            };
            xmlSettings.ValidationEventHandler += HandleValidation;

            using (var reader = XmlReader.Create(datafile.GetDatafileStream(), xmlSettings))
            {
                while (reader.Read()) { }
            }

            validation.Should().BeEmpty();

            void HandleValidation(object? sender, ValidationEventArgs e) => validation.Add(e);
        }

        private static void ReadWriteXml(string datafile)
        {
            var readNode = Deserialize();
            using var outputStream = Serialize(readNode);
            var xmlDiff = DiffXml(outputStream);
            xmlDiff.Should().BeNull();
            SourceNode Deserialize()
            {
                using var stream = datafile.GetDatafileStream();
                return stream.DeserializeSourceNodeAuto()!;
            }

            static Stream Serialize(SourceNode node)
            {
                var stream = new MemoryStream();
                node.Serialize(stream);
                stream.Position = 0;
                return stream;
            }
            string? DiffXml(Stream changedXml)
            {
                var differ = new XmlDiff(XmlDiffOptions.None);
                using var diffStream = new MemoryStream();
                using var diffWriter = XmlWriter.Create(diffStream, new XmlWriterSettings { CloseOutput = false });
                using var inputReader = XmlReader.Create(datafile.GetDatafileStream());
                using var changedReader = XmlReader.Create(changedXml);
                var result = differ.Compare(inputReader, changedReader, diffWriter);
                diffWriter.Flush();
                diffStream.Position = 0;
                using var diffReader = new StreamReader(diffStream, leaveOpen: true);
                return result ? null : diffReader.ReadToEnd();
            }
        }

        private static XmlSchemaSet ReadSchemaSet(
            RootElement rootElement,
            ValidationEventHandler validationEventHandler)
        {
            using var xsdStream = rootElement.OpenXsdStream();
            using var reader = XmlReader.Create(xsdStream!);
            var schema = XmlSchema.Read(reader, validationEventHandler);
            var set = new XmlSchemaSet();
            set.ValidationEventHandler += validationEventHandler;
            set.Add(schema!);
            set.Compile();
            return set;
        }
    }
}
