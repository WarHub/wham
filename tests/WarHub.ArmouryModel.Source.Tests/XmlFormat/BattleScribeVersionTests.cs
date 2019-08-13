using System;
using FluentAssertions;
using WarHub.ArmouryModel.Source.XmlFormat;
using Xunit;

namespace WarHub.ArmouryModel.Source.Tests.XmlFormat
{
    public class BattleScribeVersionTests
    {
        [Theory]
        [InlineData("")]
        [InlineData("v1")]
        [InlineData("v1.2")]
        [InlineData("1.a2")]
        public void Parse_throws_on_invalid_strings(string text)
        {
            Action act = () => BattleScribeVersion.Parse(text);

            act.Should().Throw<FormatException>("parsed text is invalid.");
        }

        [Theory]
        [InlineData("0.0", 0, 0)]
        [InlineData("01.05", 1, 5)]
        [InlineData("10.50", 10, 50)]
        [InlineData("1.15abba", 1, 15, "abba")]
        public void Parse_succeeds_on_valid_strings(string text, int major, int minor, string suffix = null)
        {
            var result = BattleScribeVersion.Parse(text);

            result.Should().Be(BattleScribeVersion.Create(major, minor, suffix));
        }

        [Theory]
        [InlineData(-1, 0, "major")]
        [InlineData(0, -1, "minor")]
        public void Create_throws_on_negative_numbers(int major, int minor, string argumentName)
        {
            Action act = () => BattleScribeVersion.Create(major, minor);

            act.Should().Throw<ArgumentOutOfRangeException>()
                .And.ParamName.Should().Be(argumentName);
        }

        [Fact]
        public void Equals_returns_true_on_same_object()
        {
            var v1 = BattleScribeVersion.Create(1, 0, "a");

            var result = v1.Equals(v1);

            result.Should().BeTrue("it's the same object.");
        }

        [Fact]
        public void Equals_returns_true_on_equivalent_objects()
        {
            var v1 = BattleScribeVersion.Create(1, 0, "a");
            var v2 = BattleScribeVersion.Create(1, 0, "a");

            var result = v1.Equals(v2);

            result.Should().BeTrue("it's the same version.");
        }

        [Theory]
        [InlineData("1.0", "1.0", 0)]
        [InlineData("1.1", "1.1", 0)]
        [InlineData("2.01a", "02.1a", 0)]
        [InlineData("1.0", "1.01", -1)]
        [InlineData("1.0", "1.1", -1)]
        [InlineData("1.1", "2.0", -1)]
        [InlineData("2.0", "1.1", 1)]
        [InlineData("2.0", "2.0a", 1)]
        [InlineData("2.1", "2.0a", 1)]
        public void Compare_returns_expected_result(string text1, string text2, int expectedResult)
        {
            var v1 = text1 is null ? null : BattleScribeVersion.Parse(text1);
            var v2 = text2 is null ? null : BattleScribeVersion.Parse(text2);

            var result = BattleScribeVersion.Compare(v1, v2);

            result.Should().Be(expectedResult);
        }
    }
}
