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

                var dataVersion =
                    XDocument.Load(result)
                    .Root
                    .Attribute(DataVersionManagement.BattleScribeVersionAttributeName)
                    .Value
                    .ParseBsDataVersion();
                dataVersion.Should().Be(XmlInformation.BsDataVersions.Last());
            }
        }

        [Fact]
        public void Migrating_catalogue_v_1_15_succeeds()
        {
            using (var gstStream = File.OpenRead("XmlTestDatafiles/v1_15/Space Marines - Codex (2015).cat"))
            {
                var result = DataVersionManagement.ApplyMigrations(gstStream);

                var dataVersion =
                    XDocument.Load(result)
                    .Root
                    .Attribute(DataVersionManagement.BattleScribeVersionAttributeName)
                    .Value
                    .ParseBsDataVersion();
                dataVersion.Should().Be(XmlInformation.BsDataVersions.Last());
            }
        }
    }
}
