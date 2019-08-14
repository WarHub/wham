using System.IO;
using FluentAssertions;
using Xunit;

namespace WarHub.ArmouryModel.Source.BattleScribe.Tests
{
    public class BattleScribeSerializationExtensionsTests
    {
        [Theory]
        [InlineData(BsDeserializationMode.Simple, "XmlTestDatafiles/Warhammer 40,000 8th Edition.gst")]
        [InlineData(BsDeserializationMode.MigrateOnFailure, "XmlTestDatafiles/v1_15/Warhammer40K.gst")]
        [InlineData(BsDeserializationMode.MigrateAlways, "XmlTestDatafiles/v1_15/Warhammer40K.gst")]
        public void DeserializeAuto_works_with_any_mode(BsDeserializationMode mode, string filepath)
        {
            using (var stream = File.OpenRead(filepath))
            {
                var result = stream.DeserializeAuto(mode);

                result
                    .Should().NotBeNull()
                    .And.BeOfType<GamesystemNode>();
            }
        }
    }
}
