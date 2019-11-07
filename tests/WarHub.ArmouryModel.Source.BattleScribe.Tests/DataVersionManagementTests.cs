using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using FluentAssertions;
using WarHub.ArmouryModel.Source.XmlFormat;
using Xunit;

namespace WarHub.ArmouryModel.Source.BattleScribe.Tests
{
    public class DataVersionManagementTests
    {
        [Theory]
        [InlineData(MigrationMode.None, "XmlTestDatafiles/Warhammer 40,000 8th Edition.gst")]
        [InlineData(MigrationMode.OnFailure, "XmlTestDatafiles/v1_15/Warhammer40K.gst")]
        [InlineData(MigrationMode.Always, "XmlTestDatafiles/v1_15/Warhammer40K.gst")]
        public void DeserializeAuto_works_with_any_mode(MigrationMode mode, string filepath)
        {
            using var stream = File.OpenRead(filepath);
            var result = stream.DeserializeSourceNodeAuto(mode);

            result
                .Should().NotBeNull()
                .And.BeOfType<GamesystemNode>();
        }

        [Theory]
        [InlineData("XmlTestDatafiles/v1_15/Warhammer40K.gst")]
        [InlineData("XmlTestDatafiles/v1_15/Space Marines - Codex (2015).cat")]
        public void ReadMigrated_on_old_files_succeeds(string filepath)
        {
            using var file = File.OpenRead(filepath);
            var (result, info) = DataVersionManagement.ReadMigrated(file);
            using (result)
            {
                GetVersion(result).Should().Be(info.Version);
            }
        }

        [Theory]
        [MemberData(nameof(HandledMigrationInputs))]
        public void ReadMigrated_on_handled_empty_elements_succeeds(VersionedElementInfo elementInfo)
        {
            using var emptyElementStream = CreateEmptyElementStream(elementInfo);
            var (reader, info) = DataVersionManagement.ReadMigrated(emptyElementStream);
            using (reader)
            {
                GetVersion(reader).Should().Be(info.Version);
            }
        }

        [Theory]
        [InlineData(RootElement.Catalogue, "1.15")]
        public void ReadRootElementInfo_succeeds(RootElement rootElement, string versionText)
        {
            var elementInfo =
                new VersionedElementInfo(
                    rootElement,
                    BattleScribeVersion.Parse(versionText));
            using var stream = CreateEmptyElementStream(elementInfo);
            var result = DataVersionManagement.ReadRootElementInfo(stream);

            result.Should().Be(elementInfo);
        }

        [Fact]
        public void ReadRootElementInfo_doesnt_read_to_stream_end()
        {
            using var xmlStream = new MemoryStream();
            using var writer = CreateNotClosingStreamWriter(xmlStream);
            writer.WriteLine("<roster battleScribeVersion='1.15'>");
            for (var i = 0; i < 1000; i++)
                writer.WriteLine("  <el attrib='value'>content</el>");
            writer.WriteLine("</roster>");
            writer.Flush();
            xmlStream.Position = 0;

            _ = DataVersionManagement.ReadRootElementInfo(xmlStream);

            xmlStream.Position
                .Should().BeLessThan(xmlStream.Length, "because massive streams aren't read to the end.");
        }

        private static Stream CreateEmptyElementStream(VersionedElementInfo elementInfo)
        {
            var xmlContent = string.Format(
                CultureInfo.InvariantCulture,
                "<{0} {1}='{2}' xmlns='{3}' />",
                elementInfo.Element.Info().XmlElementName,
                DataVersionManagement.BattleScribeVersionAttributeName,
                elementInfo.Version.BattleScribeString,
                elementInfo.Element.Info().Namespace);
            var xmlStream = new MemoryStream();
            using (var writer = CreateNotClosingStreamWriter(xmlStream))
            {
                writer.Write(xmlContent);
                writer.Flush();
            }
            xmlStream.Position = 0;
            return xmlStream;
        }

        private static BattleScribeVersion GetVersion(XmlReader reader)
        {
            var versionText =
                XDocument.Load(reader)
                .Root.Attribute(DataVersionManagement.BattleScribeVersionAttributeName)
                .Value;
            return BattleScribeVersion.Parse(versionText);
        }

        private static StreamWriter CreateNotClosingStreamWriter(Stream stream)
            => new StreamWriter(stream, Encoding.UTF8, 1024, leaveOpen: true);

        public static IEnumerable<object[]> HandledMigrationInputs()
        {
            return
                from migrations in XmlResources.XslMigrations
                from migration in migrations.Value
                select new object[] { migration };
        }
    }
}
