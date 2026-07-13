using System;
using System.IO;
using WarHub.ArmouryModel.Source;
using Xunit;

namespace WarHub.ArmouryModel.Source.Yaml.Tests;

public class YamlBattleScribeReaderTests
{
    // battleScribeVersion is deliberately unquoted and has a trailing zero (2.030).
    // YamlScalarNode.Value must return the raw source text "2.030" verbatim: if this
    // converter is ever switched to typed YAML deserialization, an implicit-typed
    // parser would resolve this scalar to a double and normalize it to "2.03",
    // failing the assertion below.
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
          battleScribeVersion: 2.030
          type: gameSystem
        """;

    [Fact]
    public void ReadSourceNode_parses_minimal_gamesystem()
    {
        var node = YamlBattleScribeReader.ReadSourceNode(new StringReader(MinimalGameSystem));

        var gst = Assert.IsAssignableFrom<GamesystemNode>(node);
        Assert.Equal("Test System", gst.Name);
        Assert.Equal("sys-352e-adc2-7639-d6a9", gst.Id);
        Assert.Equal("2.030", gst.BattleScribeVersion);
        var cost = Assert.Single(gst.CostTypes);
        Assert.Equal("pts", cost.Name);
    }

    [Fact]
    public void ReadSourceNode_parses_comment_and_readme_as_text_elements()
    {
        const string yaml = """
            gameSystem:
              xmlns: http://www.battlescribe.net/schema/gameSystemSchema
              id: sys-352e-adc2-7639-d6a9
              name: Test System
              revision: 1
              battleScribeVersion: "2.03"
              type: gameSystem
              comment: Awakened Dynasty
              readme: Read this before playing.
            """;

        var node = YamlBattleScribeReader.ReadSourceNode(new StringReader(yaml));

        var gst = Assert.IsAssignableFrom<GamesystemNode>(node);
        Assert.Equal("Awakened Dynasty", gst.Comment);
        Assert.Equal("Read this before playing.", gst.Readme);
    }

    [Fact]
    public void ReadSourceNode_throws_format_exception_for_sequence_of_scalars_under_collection_key()
    {
        const string yaml = """
            gameSystem:
              costTypes: [foo, bar]
              xmlns: http://www.battlescribe.net/schema/gameSystemSchema
              id: sys-352e-adc2-7639-d6a9
              name: Test System
              revision: 1
              battleScribeVersion: "2.03"
              type: gameSystem
            """;

        var ex = Assert.Throws<YamlBattleScribeFormatException>(
            () => YamlBattleScribeReader.ReadSourceNode(new StringReader(yaml)));
        Assert.Contains("costTypes", ex.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void ReadSourceNode_throws_format_exception_for_invalid_xml_attribute_name()
    {
        const string yaml = """
            gameSystem:
              xmlns: http://www.battlescribe.net/schema/gameSystemSchema
              id: sys-352e-adc2-7639-d6a9
              name: Test System
              revision: 1
              battleScribeVersion: "2.03"
              type: gameSystem
              xml:lang: en
            """;

        var ex = Assert.Throws<YamlBattleScribeFormatException>(
            () => YamlBattleScribeReader.ReadSourceNode(new StringReader(yaml)));
        Assert.Contains("xml:lang", ex.Message, StringComparison.Ordinal);
    }
}
