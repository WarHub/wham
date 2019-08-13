using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using FluentAssertions;
using WarHub.ArmouryModel.Source.XmlFormat;
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

                GetVersion(result).Should().Be(RootElement.GameSystem.Info().CurrentVersion);
            }
        }

        [Fact]
        public void Migrating_catalogue_v_1_15_succeeds()
        {
            using (var gstStream = File.OpenRead("XmlTestDatafiles/v1_15/Space Marines - Codex (2015).cat"))
            {
                var result = DataVersionManagement.ApplyMigrations(gstStream);

                GetVersion(result).Should().Be(RootElement.Catalogue.Info().CurrentVersion);
            }
        }

        [Theory]
        [MemberData(nameof(HandledMigrationInputs))]
        public void Migrating_elements_succeeds(VersionedElementInfo elementInfo)
        {
            var xmlContent = string.Format(
                "<{0} {1}='{2}' xmlns='{3}' />",
                elementInfo.RootElement.Info().XmlElementName,
                DataVersionManagement.BattleScribeVersionAttributeName,
                elementInfo.Version.BattleScribeString,
                elementInfo.RootElement.Info().Namespace);
            using (var xmlStream = new MemoryStream())
            using (var writer = new StreamWriter(xmlStream))
            {
                writer.Write(xmlContent);
                writer.Flush();
                xmlStream.Position = 0;

                var result = DataVersionManagement.ApplyMigrations(xmlStream);

                GetVersion(result).Should().Be(elementInfo.RootElement.Info().CurrentVersion);
            }
        }

        private static BattleScribeVersion GetVersion(Stream stream)
        {
            var versionText =
                XDocument.Load(stream)
                .Root.Attribute(DataVersionManagement.BattleScribeVersionAttributeName)
                .Value;
            return BattleScribeVersion.Parse(versionText);
        }

        public static IEnumerable<object[]> HandledMigrationInputs()
        {
            return
                from migrations in Resources.XslMigrations
                from migration in migrations.Value
                select new object[] { migration };
        }
    }
}
