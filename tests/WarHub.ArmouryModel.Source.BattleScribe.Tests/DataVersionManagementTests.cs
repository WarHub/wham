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
        [MemberData(nameof(VariousModesAndVariousVersionsOfFiles))]
        public void DeserializeSourceNodeAuto_with_Always_or_OnFailure_mode_opens_anything(
            BattleScribeVersion version,
            MigrationMode mode,
            string datafile)
        {
            using var stream = datafile.GetDatafileStream(version);
            var result = stream.DeserializeSourceNodeAuto(mode);
            result
                .Should().NotBeNull()
                .And.BeAssignableTo<IRootNode>();
        }

        public static IEnumerable<object[]> VariousModesAndVariousVersionsOfFiles()
        {
            return
                from version in BattleScribeVersion.WellKnownVersions.Where(x => x.IsStable)
                from mode in new[] { MigrationMode.OnFailure, MigrationMode.Always }
                from file in new[] { TestData.Gamesystem, TestData.Catalogue }
                select new object[] { version, mode, file };
        }

        [Theory]
        [InlineData(TestData.Gamesystem)]
        [InlineData(TestData.Catalogue)]
        public void ReadMigrated_on_old_files_succeeds(string datafile)
        {
            using var stream = datafile.GetDatafileStream(BattleScribeVersion.V1x15);
            var (result, info) = DataVersionManagement.ReadMigrated(stream);
            using (result)
            {
                GetVersion(result).Should().Be(info.Version!);
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
                GetVersion(reader).Should().Be(info.Version!);
            }
        }

        [Theory]
        [InlineData(RootElement.GameSystem, "1.15")]
        [InlineData(RootElement.GameSystem, "2.02b")]
        [InlineData(RootElement.Catalogue, "1.15b")]
        [InlineData(RootElement.Catalogue, "3.21a")]
        [InlineData(RootElement.DataIndex, "0.12d")]
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
                elementInfo.Version!.BattleScribeString,
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
