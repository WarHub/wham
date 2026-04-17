using FluentAssertions;
using WarHub.ArmouryModel.Concrete;
using WarHub.ArmouryModel.Source;
using Xunit;

namespace WarHub.ArmouryModel;

public class ReentrancyDetectionTests
{
    [Fact]
    public void Compilation_with_entry_links_succeeds()
    {
        // Self-completing properties should handle entry links without reentrancy issues.
        var gst = NodeFactory.Gamesystem("gst");
        var cat = NodeFactory.Catalogue(gst, "cat")
            .AddSharedSelectionEntries(
                NodeFactory.SelectionEntry("entry").Tee(out var entry))
            .AddEntryLinks(
                NodeFactory.EntryLink(entry));
        var compilation = WhamCompilation.Create(
            [SourceTree.CreateForRoot(gst), SourceTree.CreateForRoot(cat)]);

        var diagnostics = compilation.GetDiagnostics();

        diagnostics.Should().BeEmpty();
        var catalogue = compilation.GlobalNamespace.Catalogues.Single(x => !x.IsGamesystem);
        catalogue.RootContainerEntries.Should().ContainSingle()
            .Which.ReferencedEntry
            .Should().Be(catalogue.SharedSelectionEntryContainers.Single());
    }
}
