using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using WarHub.ArmouryModel.Source.XmlFormat;
using Xunit;

namespace WarHub.ArmouryModel.Source.Tests.XmlFormat
{
    public class VersionedElementInfoTests
    {
        [Theory]
        [MemberData(nameof(EmptyMigrationsVersionedElements))]
        public void AvailableMigrations_returns_empty_for_invalid_versions(
            VersionedElementInfo element)
        {
            var migrations = element.AvailableMigrations();

            migrations.Should().BeEmpty();
        }

        [Theory]
        [InlineData(RootElement.Catalogue)]
        [InlineData(RootElement.GameSystem)]
        [InlineData(RootElement.DataIndex)]
        public void AvailableMigrations_returns_migration_for_previous_migratable_version(RootElement rootElement)
        {
            var current = rootElement.Info().CurrentVersion;
            var previous = BattleScribeVersion.V2_01;
            var element = new VersionedElementInfo(rootElement, previous);

            var migrations = element.AvailableMigrations();

            migrations.Should().HaveCountGreaterOrEqualTo(1, "because we're migrating from some previous version.");
        }

        [Theory]
        [InlineData(RootElement.Catalogue, "1.0", RootElement.GameSystem, "1.0", -1)]
        [InlineData(RootElement.Catalogue, "1.0", RootElement.Catalogue, "1.0", 0)]
        [InlineData(RootElement.Catalogue, "1.0", RootElement.Catalogue, null, 1)]
        [InlineData(RootElement.Catalogue, "1.1", RootElement.Catalogue, "1.1", 0)]
        [InlineData(RootElement.Catalogue, "2.01a", RootElement.Catalogue, "02.1a", 0)]
        [InlineData(RootElement.Catalogue, "1.0", RootElement.Catalogue, "1.01", -1)]
        [InlineData(RootElement.Catalogue, "1.0", RootElement.Catalogue, "1.1", -1)]
        [InlineData(RootElement.Catalogue, "1.1", RootElement.Catalogue, "2.0", -1)]
        [InlineData(RootElement.Catalogue, "2.0", RootElement.Catalogue, "1.1", 1)]
        [InlineData(RootElement.Catalogue, "2.0", RootElement.Catalogue, "2.0a", 1)]
        [InlineData(RootElement.Catalogue, "2.1", RootElement.Catalogue, "2.0a", 1)]
        public void Compare_returns_expected_result(
            RootElement root1, string text1,
            RootElement root2, string text2,
            int expectedResult)
        {
            var element1 = CreateVersionedInfo(root1, text1);
            var element2 = CreateVersionedInfo(root2, text2);

            var result = VersionedElementInfo.Compare(element1, element2);

            result.Should().Be(expectedResult);
        }

        public static IEnumerable<object[]> EmptyMigrationsVersionedElements()
        {
            return
                (from root in RootElementInfo.AllElements.Except(new[] { RootElement.Roster })
                 from version in new[] { null, root.Info().CurrentVersion }
                 select new VersionedElementInfo(root, version))
                .Append(new VersionedElementInfo(RootElement.Roster, BattleScribeVersion.V1_15b))
                .Append(new VersionedElementInfo((RootElement)1000, BattleScribeVersion.V1_15))
                .Select(x => new object[] { x });
        }

        private static VersionedElementInfo CreateVersionedInfo(RootElement root, string versionText)
        {
            var version = versionText is null ? null : BattleScribeVersion.Parse(versionText);
            return new VersionedElementInfo(root, version);
        }
    }
}
