using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Schema;
using FluentAssertions;
using WarHub.ArmouryModel.Source.XmlFormat;
using Xunit;

namespace WarHub.ArmouryModel.Source.Tests.XmlFormat
{
    public class XmlResourcesTests
    {
        [Theory]
        [MemberData(nameof(XslMigrationVersionData))]
        public void Per_element_xsl_migration_is_available(VersionedElementInfo elementInfo)
        {
            using var migrationXslStream = elementInfo.OpenMigrationXslStream();
            migrationXslStream.Should().NotBeNull();
        }

        [Theory]
        [MemberData(nameof(ThreeRootElements))]
        public void Per_element_xsd_is_available(RootElement rootElement)
        {
            using var xsdStream = rootElement.OpenXsdStream();
            xsdStream.Should().NotBeNull();
        }

        [Theory]
        [MemberData(nameof(ThreeRootElements))]
        public void Per_element_xsd_has_no_validation_issues(RootElement rootElement)
        {
            var validationMessages = new List<ValidationEventArgs>();

            var xsd = ReadSchema(rootElement, AddEventToList);

            xsd.Should().NotBeNull();
            validationMessages.Should().BeEmpty();

            void AddEventToList(object? sender, ValidationEventArgs e) => validationMessages.Add(e);
        }

        [Theory]
        [MemberData(nameof(ThreeRootElements))]
        public void Per_element_xsd_contains_all_root_elements(RootElement rootElement)
        {
            var xmlns = rootElement.Info().Namespace;
            var schema = ReadSchema(rootElement, IgnoreEvent);
            schema.TargetNamespace.Should().Be(xmlns);
            //schema.
            schema.Elements.Names.Should().Contain(new[]
            {
                new XmlQualifiedName("gameSystem", xmlns),
                new XmlQualifiedName("catalogue", xmlns),
                new XmlQualifiedName("roster", xmlns)
            });

            static void IgnoreEvent(object? sender, ValidationEventArgs e) { }
        }

        public static IEnumerable<object[]> XslMigrationVersionData()
        {
            return
                from perElementMigrations in XmlResources.XslMigrations
                from migration in perElementMigrations.Value
                select new object[] { migration };
        }

        public static IEnumerable<object[]> ThreeRootElements()
        {
            return
                from element in new[] { RootElement.GameSystem, RootElement.Catalogue, RootElement.Roster }
                select new object[] { element };
        }

        private static XmlSchema ReadSchema(
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
            return set.Schemas().Cast<XmlSchema>().Single()!;
        }
    }
}
