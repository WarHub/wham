using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Schema;
using FluentAssertions;
using WarHub.ArmouryModel.Source.BattleScribe;
using WarHub.ArmouryModel.Source.XmlFormat;

namespace WarHub.ArmouryModel.Source.Tests.DataFormat
{
    internal static class SchemaUtils
    {
        public static XmlSchemaSet SchemaSet { get; }
            = ReadSchemaSet(RootElement.GameSystem, RootElement.Catalogue, RootElement.Roster);

        private static XmlSchemaSet ReadSchemaSet(params RootElement[] rootElements)
        {
            var set = new XmlSchemaSet();
            set.ValidationEventHandler += (s, e) => e.Should().BeNull();
            foreach (var item in rootElements)
            {
                set.Add(GetSchemaForRoot(item));
            }
            set.Compile();
            return set;

            static XmlSchema GetSchemaForRoot(RootElement root)
            {
                using var xsdStream = root.OpenXsdStream();
                using var reader = XmlReader.Create(xsdStream);
                return XmlSchema.Read(reader, (s, e) => e.Should().BeNull());
            }
        }

        public static IEnumerable<ValidationEventArgs> Validate(string xml)
        {
            // first migrate
            using var xmlReader = XmlReader.Create(new StringReader(xml));
            var (migratedReader, _) = DataVersionManagement.ReadMigrated(xmlReader);
            // then read with validation
            var messages = new List<ValidationEventArgs>();
            var settings = new XmlReaderSettings
            {
                Schemas = SchemaSet,
                ValidationType = ValidationType.Schema
            };
            settings.ValidationFlags |= XmlSchemaValidationFlags.ReportValidationWarnings;
            settings.ValidationEventHandler += (s, e) => messages.Add(e);
            using var validatingReader = XmlReader.Create(migratedReader, settings);
            return messages;
        }
    }
}
