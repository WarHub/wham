using System.Linq;
using WarHub.ArmouryModel.Source;
using Xunit;

namespace WarHub.ArmouryModel.Workspaces.Gitree.Tests
{
    public class SourceNodeToGitreeConverterTests
    {
        [Fact]
        public void AddingToEmptyBlob_AddsToCorrectList()
        {
            var addedCore = new CharacteristicTypeCore
            {
                Id = "id",
                Name = "name"
            };
            var result = SourceNodeToGitreeConverter.DatablobRewriter.AddToEmpty(addedCore.ToNode());
            Assert.Collection(result.CharacteristicTypes,
                x => Assert.Same(addedCore, ((INodeWithCore<CharacteristicTypeCore>)x).Core));
        }

        [Fact]
        public void FolderKindDroppingRewriter_ClearsCorrectList()
        {
            var characteristicType = NodeFactory.CharacteristicType("name");
            var profile = NodeFactory.ProfileType("name").AddCharacteristicTypes(characteristicType);
            var rewriter = new SourceNodeToGitreeConverter.SeparatableChildrenRemover();
            var result = (DatablobNode)NodeFactory.Datablob(
                NodeFactory.Metadata(null, null, 0),
                characteristicTypes: NodeList.Create(characteristicType),
                profileTypes: NodeList.Create(profile))
                .Accept(rewriter);
            Assert.Collection(result.CharacteristicTypes, Assert.NotNull);
            Assert.Empty(result.ProfileTypes);
        }

        [Fact]
        public void Visit_SeparatesSeparatable()
        {
            const string CostTypeId = "costType1";
            var catalogue =
                NodeFactory.Catalogue(NodeFactory.Gamesystem())
                .AddCostTypes(NodeFactory.CostType(name: "pts").WithId(CostTypeId))
                .AddRules(NodeFactory.Rule("rulename"));
            var rule = catalogue.Rules[0];
            var converter = new SourceNodeToGitreeConverter();
            var result = converter.Visit(catalogue);

            Assert.Collection(
                result.Datablob.Catalogues,
                cat =>
                {
                    Assert.Collection(
                        cat.CostTypes,
                        x => Assert.Equal(CostTypeId, x.Id));
                    Assert.Empty(cat.Rules);
                });
            Assert.Collection(
                result.Lists.Where(x => x.Items.Length > 0),
                list => Assert.Collection(
                    list.Items,
                    item => Assert.Equal(rule, item.WrappedNode)));
        }

        [Fact]
        public void Visit_ListWithDuplicateNames_AssignsUniqueNames()
        {
            const string RuleName = "Test Rule";
            var catalogue =
                NodeFactory.Catalogue(NodeFactory.Gamesystem())
                .AddRules(
                    NodeFactory.Rule(RuleName),
                    NodeFactory.Rule(RuleName));
            var converter = new SourceNodeToGitreeConverter();
            var result = converter.Visit(catalogue);

            Assert.Collection(
                result.Lists.Where(x => x.Items.Length > 0),
                ruleList => Assert.Collection(
                    ruleList.Items,
                    item => Assert.Equal(RuleName, item.Datablob.Meta.Identifier),
                    item => Assert.Equal(RuleName + " - 1", item.Datablob.Meta.Identifier)));
        }

        [Theory]
        [InlineData("Test Rule slash/ backslash\\ angleBrackets<> apos'quot\" end")]
        [InlineData("Test Rule pipe| question? asterisk* colon:")]
        [InlineData("Test Rule with emoji 👌")]
        [InlineData("Test Rule with trailing spaces   ")]
        [InlineData("Test Rule with reserved chars \x0 \x0f \x1f end")]
        public void Visit_ListWithSanitizableNames_AssignsSanitizedNames(string ruleName)
        {
            var catalogue =
                NodeFactory.Catalogue(NodeFactory.Gamesystem())
                .AddRules(NodeFactory.Rule(ruleName));
            var converter = new SourceNodeToGitreeConverter();
            var result = converter.Visit(catalogue);

            Assert.Collection(
                result.Lists.Where(x => x.Items.Length > 0),
                ruleList => Assert.Collection(
                    ruleList.Items,
                    x => Assert.Equal(ruleName.FilenameSanitize(), x.Datablob.Meta.Identifier)));
        }
    }
}
