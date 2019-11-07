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

        [Fact]
        public void Entry_link_can_contain_entries_in_schema()
        {
            const string XmlText = @"
<gameSystem id='1' name='test' revision='1' battleScribeVersion='2.01' xmlns='http://www.battlescribe.net/schema/gameSystemSchema'>
  <entryLinks>
    <entryLink id='2' targetId='123' type='selectionEntry'>
      <selectionEntries>
        <selectionEntry id='3' name='entry' type='model'/>
      </selectionEntries>
      <selectionEntryGroups>
        <selectionEntryGroup id='4' name='entry'/>
      </selectionEntryGroups>
      <entryLinks>
        <entryLink id='5' targetId='123' type='selectionEntrya'/>
      </entryLinks>
    </entryLink>
  </entryLinks>
</gameSystem>";
            var messages = SchemaUtils.Validate(XmlText);

            messages.Should().BeEmpty();
        }
    }
}
