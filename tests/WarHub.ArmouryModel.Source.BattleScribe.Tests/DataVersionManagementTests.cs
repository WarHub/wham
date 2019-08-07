using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using FluentAssertions;
using Xunit;

namespace WarHub.ArmouryModel.Source.BattleScribe.Tests
{
    public class DataVersionManagementTests
    {
        [Fact]
        public void Migrating_game_system_v_1_15_succeeds()
        {
            using (var gstStream = File.OpenRead("XmlTestDatafiles/v1_15/Warhammer40K.gst"))
            {
                var result = DataVersionManagement.ApplyMigrations(gstStream);

                XDocument.Load(result)
                    .Root
                    .Attribute(DataVersionManagement.BattleScribeVersionAttributeName)
                    .Value
                    .ParseBsDataVersion()
                    .Should().Be(XmlInformation.BsDataVersions.Last());
            }
        }

        [Fact]
        public void Migrating_catalogue_v_1_15_succeeds()
        {
            using (var gstStream = File.OpenRead("XmlTestDatafiles/v1_15/Space Marines - Codex (2015).cat"))
            {
                var result = DataVersionManagement.ApplyMigrations(gstStream);

                XDocument.Load(result)
                    .Root
                    .Attribute(DataVersionManagement.BattleScribeVersionAttributeName)
                    .Value
                    .ParseBsDataVersion()
                    .Should().Be(XmlInformation.BsDataVersions.Last());
            }
        }

        [Theory]
        [MemberData(nameof(HandledMigrationInputs))]
        public void Migrating_elements_succeeds(XmlInformation.RootElement rootElement, string versionString)
        {
            var xmlContent = string.Format(
                "<{0} {1}='{2}' />",
                rootElement.Info().XmlElementName,
                DataVersionManagement.BattleScribeVersionAttributeName,
                versionString);
            using (var xmlStream = new MemoryStream())
            using (var writer = new StreamWriter(xmlStream) { AutoFlush = true })
            {
                writer.Write(xmlContent);
                writer.Flush();
                xmlStream.Position = 0;

                var result = DataVersionManagement.ApplyMigrations(xmlStream);

                XDocument.Load(result)
                    .Root
                    .Attribute(DataVersionManagement.BattleScribeVersionAttributeName)
                    .Value
                    .ParseBsDataVersion()
                    .Should().Be(XmlInformation.BsDataVersions.Last());
            }
        }

        public static IEnumerable<object[]> HandledMigrationInputs()
        {
            return
                from root in new[] { XmlInformation.RootElement.GameSystem, XmlInformation.RootElement.Catalogue }
                from targetVersion in root.Info().AvailableMigrations(XmlInformation.BsDataVersion.v1_15)
                select new object[] { root, targetVersion.Info().DisplayString };
        }
    }
}
