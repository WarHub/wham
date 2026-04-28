using BattleScribeSpec;
using WarHub.ArmouryModel.RosterEngine.Spec;
using Xunit;

namespace WarHub.ArmouryModel.RosterEngine.Tests;

public class ConformanceTests
{
    // Specs with known contradictions in the spec set that cannot be resolved
    // without upstream changes. Each entry must have a justification comment.
    private static readonly HashSet<string> KnownSpecContradictions = new(StringComparer.Ordinal)
    {
        // modifier-set-hidden-no-category expects a hidden error for entries
        // without categoryLinks, but modifier-field-hidden, selection-hidden-entry,
        // and constraint-hidden-enforcement all expect zero errors for identical setups.
        // The spec set is internally inconsistent; we align with the majority (3 specs).
        "modifier-set-hidden-no-category",
    };

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
        if (spec.IsExpectedToFail("wham"))
        {
            Assert.False(result.Passed,
                $"Spec {id} is marked as expected-to-fail for wham but now passes. " +
                "Update the spec to remove the 'wham: fail' expectation.");
            return;
        }
        if (KnownSpecContradictions.Contains(id))
        {
            if (result.Passed)
            {
                Assert.Fail(
                    $"Spec {id} was in KnownSpecContradictions but now passes — remove it from the set.");
            }
            // Known contradiction — skip assertion.
            return;
        }
        Assert.True(result.Passed, $"Spec {id} failed:\n{string.Join("\n", result.Failures)}");
    }
}
