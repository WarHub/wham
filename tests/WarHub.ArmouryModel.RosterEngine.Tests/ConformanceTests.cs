using BattleScribeSpec;
using Xunit;

namespace WarHub.ArmouryModel.RosterEngine.Tests;

public class ConformanceTests
{
    public static IEnumerable<object[]> AllSpecs()
    {
        foreach (var (resourceName, id, category) in SpecLoader.DiscoverEmbeddedSpecs())
        {
            var spec = SpecLoader.LoadEmbedded(resourceName);
            // Skip DataSource specs (real-world data) for now
            if (spec.Setup.DataSource is { Length: > 0 }) continue;
            yield return [id, spec];
        }
    }

    [Theory]
    [MemberData(nameof(AllSpecs))]
    public void Spec(string id, SpecFile spec)
    {
        using var engine = new WhamRosterEngine();
        var runner = new SpecRunner(engine, engineName: "wham");
        var result = runner.Run(spec);
        Assert.True(result.Passed, $"Spec '{id}' failed:\n{string.Join("\n", result.Failures)}");
    }
}
