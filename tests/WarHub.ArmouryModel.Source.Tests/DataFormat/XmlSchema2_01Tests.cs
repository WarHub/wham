using System.Linq;
using FluentAssertions;
using Xunit;
using static WarHub.ArmouryModel.Source.NodeFactory;

namespace WarHub.ArmouryModel.Source.Tests.DataFormat
{
    public class XmlSchema2_01Tests
    {
        [Fact]
        public void EntryLink_can_contain_entries()
        {
            var link = EntryLink(SelectionEntry());
            var result = link
                .AddSelectionEntries(
                    SelectionEntry())
                .AddSelectionEntryGroups(
                    SelectionEntryGroup())
                .AddEntryLinks(
                    EntryLink(
                        SelectionEntry()));
            new IListNode[]
            {
                result.SelectionEntries,
                result.SelectionEntryGroups,
                result.EntryLinks
            }
                .Select(x => x.NodeList.Count)
                .Should()
                .AllBeEquivalentTo(1);
        }
    }
}
