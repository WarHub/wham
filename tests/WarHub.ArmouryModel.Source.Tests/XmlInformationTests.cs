using System.Collections.Generic;
using System.Xml;
using System.Xml.Schema;
using FluentAssertions;
using Xunit;
using System.Linq;
using static WarHub.ArmouryModel.Source.XmlInformation;

namespace WarHub.ArmouryModel.Source.Tests
{
    public class XmlInformationTests
    {
        [Theory]
        [MemberData(nameof(XslMigrationVersionData))]
        public void Per_element_xsl_migration_is_available(RootElement rootElement, BsDataVersion dataVersion)
        {
            using (var migrationXslStream = OpenMigrationXslStream(rootElement, dataVersion))
            {
                migrationXslStream.Should().NotBeNull();
            }
        }

        [Theory]
        [MemberData(nameof(ThreeRootElements))]
        public void Per_element_xsd_is_available(RootElement rootElement)
        {
            using (var xsdStream = OpenXsdStream(rootElement))
            {
                xsdStream.Should().NotBeNull();
            }
        }

        [Theory]
        [MemberData(nameof(ThreeRootElements))]
        public void Per_element_xsd_has_no_validation_issues(RootElement rootElement)
        {
            var validationMessages = new List<ValidationEventArgs>();

            var xsd = ReadSchema(rootElement, AddEventToList);

            xsd.Should().NotBeNull();
            validationMessages.Should().BeEmpty();

            void AddEventToList(object sender, ValidationEventArgs e) => validationMessages.Add(e);
        }

        [Theory]
        [MemberData(nameof(ThreeRootElements))]
        public void Per_element_xsd_contains_all_root_elements(RootElement rootElement)
        {
            var xmlns = Namespace(rootElement);
            var schema = ReadSchema(rootElement, IgnoreEvent);
            schema.TargetNamespace.Should().Be(xmlns);
            //schema.
            schema.Elements.Names.Should().Contain(new[]
            {
                new XmlQualifiedName("gameSystem", xmlns),
                new XmlQualifiedName("catalogue", xmlns),
                new XmlQualifiedName("roster", xmlns)
            });

            void IgnoreEvent(object sender, ValidationEventArgs e) { }
        }

        public static IEnumerable<object[]> XslMigrationVersionData()
        {
            return
                from version in BsDataVersions
                from element in new[] { RootElement.GameSystem, RootElement.Catalogue }
                select new object[] { element, version };
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
            using (var xsdStream = OpenXsdStream(rootElement))
            {
                var schema = XmlSchema.Read(xsdStream, validationEventHandler);
                var set = new XmlSchemaSet();
                set.ValidationEventHandler += validationEventHandler;
                set.Add(schema);
                set.Compile();
                return set.Schemas().Cast<XmlSchema>().Single();
            }
        }
    }
}
