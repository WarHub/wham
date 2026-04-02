using BattleScribeSpec;
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
            yield return [id, resourceName];
        }
    }

    [Theory]
    [MemberData(nameof(AllSpecs))]
    public void Spec(string id, string resourceName)
    {
        var spec = SpecLoader.LoadEmbedded(resourceName);
        using var engine = new SpecRosterEngineAdapter();
        var runner = new SpecRunner(engine, engineName: "wham");
        var result = runner.Run(spec);
        Assert.True(result.Passed, $"Spec {id} failed:\n{string.Join("\n", result.Failures)}");
    }
}
