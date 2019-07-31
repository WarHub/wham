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
        [Theory]
        [InlineData(XmlInformation.RootElement.Catalogue)]
        [InlineData(XmlInformation.RootElement.GameSystem)]
        [InlineData(XmlInformation.RootElement.Roster)]
        public void Per_element_xsd_is_available(XmlInformation.RootElement rootElement)
        {
            using (var xsdStream = XmlInformation.OpenXsdStream(rootElement))
            {
                xsdStream.Should().NotBeNull();
            }
        }

        [Theory]
        [InlineData(XmlInformation.RootElement.Catalogue)]
        [InlineData(XmlInformation.RootElement.GameSystem)]
        [InlineData(XmlInformation.RootElement.Roster)]
        public void Per_element_xsd_has_no_validation_issues(XmlInformation.RootElement rootElement)
        {
            var validationMessages = new List<ValidationEventArgs>();

            var xsd = ReadSchema(rootElement, AddEventToList);

            xsd.Should().NotBeNull();
            validationMessages.Should().BeEmpty();

            void AddEventToList(object sender, ValidationEventArgs e) => validationMessages.Add(e);
        }

        [Theory]
        [InlineData(XmlInformation.RootElement.Catalogue)]
        [InlineData(XmlInformation.RootElement.GameSystem)]
        [InlineData(XmlInformation.RootElement.Roster)]
        public void Per_element_xsd_contains_Roster_Catalogue_Gamesystem_root_elements(XmlInformation.RootElement rootElement)
        {
            var xmlns = XmlInformation.Namespace(rootElement);
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

        private static XmlSchema ReadSchema(
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
                return set.Schemas().Cast<XmlSchema>().Single();
            }
        }
    }
}
