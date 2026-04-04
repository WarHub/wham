using FluentAssertions;
using WarHub.ArmouryModel.Concrete;
using WarHub.ArmouryModel.Source;
using Xunit;

namespace WarHub.ArmouryModel;

public class ReentrancyDetectionTests
{
    [Fact]
    public void DetectBindingReentrancy_enabled_normal_compilation_succeeds()
    {
        // A normal compilation should work fine even with reentrancy detection on.
        var gst = NodeFactory.Gamesystem("gst");
        var cat = NodeFactory.Catalogue(gst, "cat")
            .AddSharedSelectionEntries(
                NodeFactory.SelectionEntry("entry").Tee(out var entry))
            .AddEntryLinks(
                NodeFactory.EntryLink(entry));
        var options = new WhamCompilationOptions { DetectBindingReentrancy = true };
        var compilation = WhamCompilation.Create(
            [SourceTree.CreateForRoot(gst), SourceTree.CreateForRoot(cat)],
            options);

        var diagnostics = compilation.GetDiagnostics();

        diagnostics.Should().BeEmpty();
        var catalogue = compilation.GlobalNamespace.Catalogues.Single(x => !x.IsGamesystem);
        catalogue.RootContainerEntries.Should().ContainSingle()
            .Which.ReferencedEntry
            .Should().Be(catalogue.SharedSelectionEntryContainers.Single());
    }

    [Fact]
    public void DetectBindingReentrancy_default_is_false()
    {
        var options = new WhamCompilationOptions();
        options.DetectBindingReentrancy.Should().BeFalse();
    }
}
