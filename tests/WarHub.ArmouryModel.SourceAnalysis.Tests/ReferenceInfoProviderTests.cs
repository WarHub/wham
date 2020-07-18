using System;
using System.Collections.Generic;
using System.Text;
using FluentAssertions;
using WarHub.ArmouryModel.Source;
using Xunit;

namespace WarHub.ArmouryModel.SourceAnalysis.Tests
{
    public class ReferenceInfoProviderTests
    {
        [Fact]
        public void EntryLinkToSharedEntryIsFound()
        {
            var gst = NodeFactory.Gamesystem();
            var selection = NodeFactory.SelectionEntry();
            var cat = NodeFactory.Catalogue(gst)
                .AddSharedSelectionEntries(selection)
                .AddEntryLinks(
                    NodeFactory.EntryLink(selection));
            var ctx = GamesystemContext.CreateSingle(gst, cat);
            var refs = ctx.GetReferences(cat.SharedSelectionEntries[0]);

            refs.InLinkTargetId.Should().HaveCount(1);
        }

        [Fact]
        public void EntryLinkToGamesystemEntryIsFound()
        {
            var gst = NodeFactory.Gamesystem()
                .AddSharedSelectionEntries(
                    NodeFactory.SelectionEntry());
            var cat = NodeFactory.Catalogue(gst)
                .AddEntryLinks(
                    NodeFactory.EntryLink(gst.SharedSelectionEntries[0]));
            var ctx = GamesystemContext.CreateSingle(gst, cat);

            var refs = ctx.GetReferences(gst.SharedSelectionEntries[0]);

            refs.InLinkTargetId.Should().HaveCount(1);
        }
    }
}
