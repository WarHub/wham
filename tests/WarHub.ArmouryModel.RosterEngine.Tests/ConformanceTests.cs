using BattleScribeSpec;
using BattleScribeSpec.Roster;
using WarHub.ArmouryModel.RosterEngine.Spec;
using Xunit;

namespace WarHub.ArmouryModel.RosterEngine.Tests;

public class ConformanceTests
{
    public static IEnumerable<object[]> AllSpecs()
    {
        foreach (var (resourceName, id, category) in SpecLoader.DiscoverEmbeddedSpecs())
        {
            SpecFile spec;
            try
            {
                spec = SpecLoader.LoadEmbedded(resourceName);
            }
            catch
            {
                continue;
            }
            if (spec.Setup.DataSource is { Length: > 0 }) continue;
            if (spec.ShouldSkip("wham")) continue;
            yield return [spec.Id, resourceName];
        }
    }

    [Theory]
    [MemberData(nameof(AllSpecs))]
    public void Spec(string id, string resourceName)
    {
        var spec = SpecLoader.LoadEmbedded(resourceName);
        using var engine = new SpecRosterEngineAdapter();
        var runner = new RosterRunner(engine, engineName: "wham");
        var result = runner.Run(spec);
        if (spec.IsExpectedToFail("wham"))
        {
            Assert.False(result.Passed,
                $"Spec {id} is marked as expected-to-fail for wham but now passes. " +
                "Update the spec to remove the 'wham: fail' expectation.");
            return;
        }
        Assert.True(result.Passed, $"Spec {id} failed:\n{string.Join("\n", result.Failures)}");
    }
}
