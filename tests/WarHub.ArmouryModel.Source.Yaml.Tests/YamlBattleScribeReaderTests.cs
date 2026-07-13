using System.IO;
using WarHub.ArmouryModel.Source;
using Xunit;

namespace WarHub.ArmouryModel.Source.Yaml.Tests;

public class YamlBattleScribeReaderTests
{
    private const string MinimalGameSystem = """
        gameSystem:
          costTypes:
            - name: pts
              id: 51b2-306e-1021-d207
              defaultCostLimit: -1
          xmlns: http://www.battlescribe.net/schema/gameSystemSchema
          id: sys-352e-adc2-7639-d6a9
          name: Test System
          revision: 1
          battleScribeVersion: "2.03"
          type: gameSystem
        """;

    [Fact]
    public void ReadSourceNode_parses_minimal_gamesystem()
    {
        var node = YamlBattleScribeReader.ReadSourceNode(new StringReader(MinimalGameSystem));

        var gst = Assert.IsAssignableFrom<GamesystemNode>(node);
        Assert.Equal("Test System", gst.Name);
        Assert.Equal("sys-352e-adc2-7639-d6a9", gst.Id);
        Assert.Equal("2.03", gst.BattleScribeVersion);
        var cost = Assert.Single(gst.CostTypes);
        Assert.Equal("pts", cost.Name);
    }
}
