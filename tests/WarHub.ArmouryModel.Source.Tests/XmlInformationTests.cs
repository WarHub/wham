using System.Collections.Generic;
using System.Xml;
using System.Xml.Schema;
using FluentAssertions;
using Xunit;
using System.Linq;

namespace WarHub.ArmouryModel.Source.Tests
{
    public class XmlInformationTests
    {
        [Fact]
        public void Catalogue_xsd_is_available()
        {
            using (var xsdStream = XmlInformation.OpenCatalogueXmlSchemaDefinitionStream())
            {
                xsdStream.Should().NotBeNull();
            }
        }

        [Fact]
        public void Catalogue_xsd_has_no_validation_issues()
        {
            var validationMessages = new List<ValidationEventArgs>();

            var xsd = ReadSchema(AddEventToList);

            xsd.Should().NotBeNull();
            validationMessages.Should().BeEmpty();

            void AddEventToList(object sender, ValidationEventArgs e) => validationMessages.Add(e);
        }

        [Fact]
        public void Catalogue_xsd_contains_Roster_Catalogue_Gamesystem_root_elements()
        {
            var schema = ReadSchema(IgnoreEvent);

            schema.TargetNamespace.Should().Be(XmlInformation.Namespaces.CatalogueXmlns);
            //schema.
            schema.Elements.Names.Should().Contain(new[]
            {
                new XmlQualifiedName("gameSystem", XmlInformation.Namespaces.CatalogueXmlns),
                new XmlQualifiedName("catalogue", XmlInformation.Namespaces.CatalogueXmlns),
                new XmlQualifiedName("roster", XmlInformation.Namespaces.CatalogueXmlns)
            });

            void IgnoreEvent(object sender, ValidationEventArgs e) { }
        }

        private static XmlSchema ReadSchema(ValidationEventHandler validationEventHandler)
        {
            using (var xsdStream = XmlInformation.OpenCatalogueXmlSchemaDefinitionStream())
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
