using BattleScribeSpec.Roster;
using WarHub.ArmouryModel.RosterEngine.Spec;
using Xunit;

namespace WarHub.ArmouryModel.RosterEngine.Spec.Tests;

public class SetupFromFilesTests
{
    private const string GstYaml = """
        gameSystem:
          costTypes:
            - name: pts
              id: ct-pts
              defaultCostLimit: -1
          forceEntries:
            - name: Army
              id: fe-army
              hidden: false
          xmlns: http://www.battlescribe.net/schema/gameSystemSchema
          id: gs-test
          name: Test System
          revision: 1
          battleScribeVersion: "2.03"
          type: gameSystem
        """;

    private const string CatYaml = """
        catalogue:
          selectionEntries:
            - costs:
                - name: pts
                  typeId: ct-pts
                  value: 20
              type: unit
              import: true
              name: Test Unit
              hidden: false
              id: se-unit
          xmlns: http://www.battlescribe.net/schema/catalogueSchema
          id: cat-test
          name: Test Catalogue
          revision: 1
          battleScribeVersion: "2.03"
          gameSystemId: gs-test
          gameSystemRevision: 1
        """;

    [Fact]
    public void SetupFromFiles_yaml_data_supports_roster_actions()
    {
        using IRosterEngine engine = new SpecRosterEngineAdapter();
        engine.SetupFromFiles([("Test System.yaml", GstYaml), ("Test Catalogue.yaml", CatYaml)]);
        var force = engine.AddForce("fe-army", "cat-test");
        Assert.NotNull(force.ForceId);
        engine.SelectEntry(force.ForceId!, "se-unit");
        var state = engine.GetRosterState();
        Assert.Contains(state.Costs, c => c.Value == 20);
    }

    [Fact]
    public void SetupFromFiles_malformed_second_file_reports_its_name()
    {
        const string malformedYaml = """
            catalogue:
              selectionEntries: [not-a-mapping]
              xmlns: http://www.battlescribe.net/schema/catalogueSchema
              id: cat-bad
              name: Bad Catalogue
              revision: 1
              battleScribeVersion: "2.03"
              gameSystemId: gs-test
              gameSystemRevision: 1
            """;

        using IRosterEngine engine = new SpecRosterEngineAdapter();
        var ex = Assert.Throws<InvalidOperationException>(() =>
            engine.SetupFromFiles([("Test System.yaml", GstYaml), ("Bad Catalogue.yaml", malformedYaml)]));

        Assert.Contains("Bad Catalogue.yaml", ex.Message, StringComparison.Ordinal);
        Assert.NotNull(ex.InnerException);
    }
}
