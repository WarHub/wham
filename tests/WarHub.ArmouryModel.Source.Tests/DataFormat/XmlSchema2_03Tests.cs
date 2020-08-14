using System.IO;
using System.Xml;
using System.Xml.Serialization;
using FluentAssertions;
using WarHub.ArmouryModel.Source.BattleScribe;
using WarHub.ArmouryModel.Source.XmlFormat;
using Xunit;
using static WarHub.ArmouryModel.Source.NodeFactory;

namespace WarHub.ArmouryModel.Source.Tests.DataFormat
{
    public class XmlSchema2_03Tests
    {
        private static string BsVersion_2_03 { get; } = BattleScribeVersion.V2x03.BattleScribeString;

        private const string GamesystemWithReadmeAndComment = @"
<gameSystem id='1' name='test' revision='1' battleScribeVersion='2.03' xmlns='http://www.battlescribe.net/schema/gameSystemSchema'>
  <comment>This is comment</comment>
  <readme>This is readme content
spanning multiple lines</readme>
  <publications />
</gameSystem>";
        private const string CatalogueWithReadmeAndComment = @"
<catalogue id='1' name='test' revision='1' gameSystemId='123' battleScribeVersion='2.03' xmlns='http://www.battlescribe.net/schema/catalogueSchema'>
  <comment>This is comment</comment>
  <readme>This is readme content
spanning multiple lines</readme>
  <publications />
</catalogue>";

        [Fact]
        public void Readme_in_gamesystem_is_readable()
        {
            using var xmlReader = XmlReader.Create(new StringReader(GamesystemWithReadmeAndComment));

            var root = BattleScribeXmlSerializer.Instance.DeserializeGamesystem(x => x.Deserialize(xmlReader));

            root.Readme.Should().MatchRegex(@"content\nspanning");
        }

        [Fact]
        public void Readme_in_gamesystem_is_schema_validated()
        {
            var messages = SchemaUtils.Validate(GamesystemWithReadmeAndComment);

            messages.Should().BeEmpty();
        }

        [Fact]
        public void Readme_in_catalogue_is_readable()
        {
            using var xmlReader = XmlReader.Create(new StringReader(CatalogueWithReadmeAndComment));

            var root = BattleScribeXmlSerializer.Instance.DeserializeCatalogue(x => x.Deserialize(xmlReader));

            root.Readme.Should().MatchRegex(@"content\nspanning");
        }

        [Fact]
        public void Readme_in_catalogue_is_schema_validated()
        {
            var messages = SchemaUtils.Validate(CatalogueWithReadmeAndComment);

            messages.Should().BeEmpty();
        }

        [Fact]
        public void RosterTags_in_roster_are_schema_validated()
        {
            var roster = Roster(Gamesystem())
                .WithBattleScribeVersion(BsVersion_2_03)
                .AddTags(
                    RosterTag("tag1"),
                    RosterTag("tag2"));

            var messages = SchemaUtils.Validate(roster);

            messages.Should().BeEmpty();
        }

        [Fact]
        public void Comment_is_schema_validated()
        {
            var gst = Gamesystem();
            var entryWithChildren = SelectionEntry()
                .AddCategoryLinks(CategoryLink(CategoryEntry()))
                .AddConstraints(Constraint())
                .AddCosts(Cost(CostType()))
                .AddModifierGroups(ModifierGroup())
                .AddModifiers(
                    Modifier()
                    .AddConditionGroups(ConditionGroup())
                    .AddConditions(Condition())
                    .AddRepeats(Repeat()));
            var cat = Catalogue(gst)
                .AddCatalogueLinks(
                    CatalogueLink(Catalogue(gst)))
                .AddCategoryEntries(
                    CategoryEntry())
                .AddCostTypes(
                    CostType())
                .AddEntryLinks(
                    EntryLink(entryWithChildren))
                .AddForceEntries(
                    ForceEntry())
                .AddInfoLinks(
                    InfoLink(Rule()))
                .AddProfileTypes(
                    ProfileType()
                    .AddCharacteristicTypes(
                        CharacteristicType()))
                .AddPublications(
                    Publication())
                .AddRules(
                    Rule())
                .AddSelectionEntries(
                    SelectionEntry(),
                    entryWithChildren)
                .AddSharedInfoGroups(
                    InfoGroup())
                .AddSharedProfiles(
                    Profile(ProfileType()))
                .AddSharedRules(
                    Rule())
                .AddSharedSelectionEntries(
                    SelectionEntry())
                .AddSharedSelectionEntryGroups(
                    SelectionEntryGroup());
            var catWithComs = (CatalogueNode)new CommentRewriter().Visit(cat)!;

            var messages = SchemaUtils.Validate(catWithComs);

            messages.Should().BeEmpty();
        }

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

        private class CommentRewriter : SourceRewriter
        {
            public override SourceNode? Visit(SourceNode? node) =>
                base.Visit(node is CommentableNode com ? com.WithComment("txt") : node);
        }
    }
}
