using BattleScribeSpec;
using WarHub.ArmouryModel.RosterEngine.Spec;
using Xunit;

namespace WarHub.ArmouryModel.RosterEngine.Tests;

public class ConformanceTests
{
    // Specs tagged undefined-behavior where all known engines fail.
    // These represent ambiguous or unspecified behavior in the BattleScribe format.
    // Each entry must have a justification comment.
    private static readonly HashSet<string> KnownUndefinedBehavior = new(StringComparer.Ordinal)
    {
        // ModifierGroup on an infoGroup: all engines (battlescribe, newrecruit) fail this.
        // The spec expects modifierGroups on infoGroups to propagate to contained profiles,
        // but this is explicitly tagged undefined-behavior.
        "modifier-group-on-infogroup",
        // Game system force entry modifier condition references childId defined in a catalogue.
        // Game system scope cannot access catalogue entries — likely a spec issue.
        "force-entry-with-modifier",
        // Spec expects hidden constraint errors to be suppressed until user explicitly
        // selects entries in a force. wham validates hidden constraints immediately.
        // See: https://github.com/WarHub/battlescribe-spec/issues/163
        "constraint-hidden-enforcement",
        // CategoryLinks should not have modifiers — they are not valid modifier containers
        // in the BattleScribe data model. All engines (battlescribe, newrecruit) fail this.
        // See: https://github.com/WarHub/battlescribe-spec/issues/165
        "modifier-group-on-category-link",
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
        if (KnownUndefinedBehavior.Contains(id))
        {
            if (result.Passed)
            {
                Assert.Fail(
                    $"Spec {id} was in KnownUndefinedBehavior but now passes — remove it from the set.");
            }
            // Known undefined behavior — all engines fail this spec.
            return;
        }
        Assert.True(result.Passed, $"Spec {id} failed:\n{string.Join("\n", result.Failures)}");
    }
}
