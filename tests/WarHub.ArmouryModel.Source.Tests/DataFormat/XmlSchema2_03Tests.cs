using System.IO;
using System.Xml;
using System.Xml.Serialization;
using FluentAssertions;
using WarHub.ArmouryModel.Source.BattleScribe;
using Xunit;
using static WarHub.ArmouryModel.Source.NodeFactory;

namespace WarHub.ArmouryModel.Source.Tests.DataFormat
{
    public class XmlSchema2_03Tests
    {
        [Theory]
        [InlineData("add", ModifierKind.Add)]
        [InlineData("remove", ModifierKind.Remove)]
        [InlineData("set-primary", ModifierKind.SetPrimary)]
        [InlineData("unset-primary", ModifierKind.UnsetPrimary)]
        public void ModifierKind_correctly_parses_category_kinds(string kindString, ModifierKind kindValue)
        {
            var xml = $"<modifier type=\"{kindString}\" />";
            using var reader = XmlReader.Create(new StringReader(xml));
            var modifier = (ModifierCore.Builder)new XmlSerializer(typeof(ModifierCore.Builder)).Deserialize(reader);

            modifier.Type.Should().Be(kindValue);
        }

        [Theory]
        [InlineData(ModifierKind.Add)]
        [InlineData(ModifierKind.Remove)]
        [InlineData(ModifierKind.SetPrimary)]
        [InlineData(ModifierKind.UnsetPrimary)]
        public void ModifierKind_additions_validated_by_schema(ModifierKind kindValue)
        {
            var data =
                Gamesystem()
                .AddSelectionEntries(
                    SelectionEntry()
                    .AddModifiers(
                        Modifier(type: kindValue)));
            using var memStream = new MemoryStream();
            data.Serialize(memStream);
            memStream.Position = 0;
            var gst = memStream.DeserializeGamesystem();

            var modifier = gst.SelectionEntries[0].Modifiers[0];

            modifier.Type.Should().Be(kindValue);
        }

        [Fact]
        public void EntryLink_can_contain_costs()
        {
            var link = EntryLink(SelectionEntry());
            var result = link.AddCosts(Cost(CostType(), 1));
            result.Costs[0].Value.Should().Be(1);
        }

        [Fact]
        public void Entry_link_can_contain_cost_in_schema()
        {
            const string XmlText = @"
<gameSystem id='1' name='test' revision='1' battleScribeVersion='2.01' xmlns='http://www.battlescribe.net/schema/gameSystemSchema'>
  <entryLinks>
    <entryLink id='2' targetId='123' type='selectionEntry'>
      <costs>
        <cost typeId='3' name='c1' value='123'/>
      </costs>
    </entryLink>
  </entryLinks>
</gameSystem>";
            var messages = SchemaUtils.Validate(XmlText);

            messages.Should().BeEmpty();
        }
    }
}
