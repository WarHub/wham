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

    private const string CatalogueWithModifiers = """
        catalogue:
          sharedSelectionEntries:
            - modifiers:
                - conditionGroups:
                    - conditions:
                        - type: lessThan
                          value: 1
                          field: forces
                          scope: roster
                          childId: cac3-71d1-ea4b-795d
                          shared: true
                          id: 1c00-f001-2a27-52b7
                          includeChildSelections: true
                      type: and
                  type: set
                  value: true
                  field: hidden
              costs:
                - name: pts
                  typeId: 51b2-306e-1021-d207
                  value: 20
              constraints:
                - type: max
                  value: 1
                  field: selections
                  scope: roster
                  shared: true
                  id: f2e6-938b-9ff0-37c7
                  includeChildSelections: true
              profiles:
                - characteristics:
                    - $text: While the bearer is leading a unit, models in that unit have the Stealth ability.
                      name: Description
                      typeId: 9082-un23-lead-abil
                  typeId: 5570-9av2
                  name: Veil of Darkness
                  id: prof-veil-1
              comment: Awakened Dynasty
              type: upgrade
              import: true
              name: Veil of Darkness
              hidden: false
              id: a562-e37c-3a4d-d152
          xmlns: http://www.battlescribe.net/schema/catalogueSchema
          id: cat-necrons-test
          name: Test Necrons
          revision: 1
          battleScribeVersion: "2.03"
          gameSystemId: sys-352e-adc2-7639-d6a9
          gameSystemRevision: 1
        """;

    [Fact]
    public void ReadSourceNode_preserves_modifiers_costs_constraints_and_text()
    {
        var node = YamlBattleScribeReader.ReadSourceNode(new StringReader(CatalogueWithModifiers));

        var cat = Assert.IsAssignableFrom<CatalogueNode>(node);
        var entry = Assert.Single(cat.SharedSelectionEntries);
        Assert.Equal("Veil of Darkness", entry.Name);
        Assert.Equal(20m, Assert.Single(entry.Costs).Value);
        var modifier = Assert.Single(entry.Modifiers);
        Assert.Equal("true", modifier.Value);
        var conditionGroup = Assert.Single(modifier.ConditionGroups);
        Assert.Equal(ConditionGroupKind.And, conditionGroup.Type);
        var condition = Assert.Single(conditionGroup.Conditions);
        Assert.Equal("cac3-71d1-ea4b-795d", condition.ChildId);
        var constraint = Assert.Single(entry.Constraints);
        Assert.Equal("selections", constraint.Field);
        Assert.Equal(1m, constraint.Value);
        Assert.Equal("f2e6-938b-9ff0-37c7", constraint.Id);
        Assert.Equal("Awakened Dynasty", entry.Comment);
        var characteristic = Assert.Single(Assert.Single(entry.Profiles).Characteristics);
        Assert.Contains("Stealth ability", characteristic.Value, StringComparison.Ordinal);
    }

    [Fact]
    public void ReadSourceNode_throws_on_unknown_collection_key()
    {
        const string bad = """
            catalogue:
              notARealCollection:
                - name: x
              xmlns: http://www.battlescribe.net/schema/catalogueSchema
              id: c1
              name: Bad
              revision: 1
              battleScribeVersion: "2.03"
              gameSystemId: g1
              gameSystemRevision: 1
            """;
        var ex = Assert.Throws<YamlBattleScribeFormatException>(
            () => YamlBattleScribeReader.ReadSourceNode(new StringReader(bad)));
        Assert.Contains("notARealCollection", ex.Message, StringComparison.Ordinal);
    }
}
