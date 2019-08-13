using FluentAssertions;
using WarHub.ArmouryModel.Source.XmlFormat;
using Xunit;

namespace WarHub.ArmouryModel.Source.Tests.XmlFormat
{
    public class VersionedElementInfoTests
    {
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
            var v1 = text1 is null ? null : BattleScribeVersion.Parse(text1);
            var v2 = text2 is null ? null : BattleScribeVersion.Parse(text2);
            var element1 = new VersionedElementInfo(root1, v1);
            var element2 = new VersionedElementInfo(root2, v2);

            var result = VersionedElementInfo.Compare(element1, element2);

            result.Should().Be(expectedResult);
        }
    }
}
