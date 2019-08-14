using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using FluentAssertions;
using WarHub.ArmouryModel.Source.XmlFormat;
using Xunit;

namespace WarHub.ArmouryModel.Source.BattleScribe.Tests
{
    public class DataVersionManagementTests
    {
        [Theory]
        [InlineData("XmlTestDatafiles/v1_15/Warhammer40K.gst")]
        [InlineData("XmlTestDatafiles/v1_15/Space Marines - Codex (2015).cat")]
        public void Migrate_on_old_files_succeeds(string filepath)
        {
            var (result, inputInfo) = DataVersionManagement.Migrate(() => File.OpenRead(filepath));
            using (result)
            {
                GetVersion(result).Should().Be(inputInfo.Element.Info().CurrentVersion);
            }
        }

        [Theory]
        [MemberData(nameof(HandledMigrationInputs))]
        public void Migrate_on_handled_empty_elements_succeeds(VersionedElementInfo elementInfo)
        {
            var (result, inputInfo) = DataVersionManagement.Migrate(
                () => CreateEmptyElementStream(elementInfo));
            using (result)
            {
                GetVersion(result).Should().Be(inputInfo.Element.Info().CurrentVersion);
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
            using (var stream = CreateEmptyElementStream(elementInfo))
            {
                var result = DataVersionManagement.ReadRootElementInfo(stream);

                result.Should().Be(elementInfo);
            }
        }

        [Fact]
        public void ReadRootElementInfo_doesnt_read_to_stream_end()
        {
            using (var xmlStream = new MemoryStream())
            using (var writer = CreateNotClosingStreamWriter(xmlStream))
            {
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
        }

        private static Stream CreateEmptyElementStream(VersionedElementInfo elementInfo)
        {
            var xmlContent = string.Format(
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

        private static BattleScribeVersion GetVersion(Stream stream)
        {
            var versionText =
                XDocument.Load(stream)
                .Root.Attribute(DataVersionManagement.BattleScribeVersionAttributeName)
                .Value;
            return BattleScribeVersion.Parse(versionText);
        }

        private static StreamWriter CreateNotClosingStreamWriter(Stream stream)
            => new StreamWriter(stream, Encoding.UTF8, 1024, leaveOpen: true);

        public static IEnumerable<object[]> HandledMigrationInputs()
        {
            return
                from migrations in Resources.XslMigrations
                from migration in migrations.Value
                select new object[] { migration };
        }
    }
}
