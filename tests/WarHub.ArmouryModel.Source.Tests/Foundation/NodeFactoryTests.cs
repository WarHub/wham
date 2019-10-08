using System.Collections.Generic;
using FluentAssertions;
using Xunit;

namespace WarHub.ArmouryModel.Source.Tests.Foundation
{
    public class NodeFactoryTests
    {
        [Theory]
        [MemberData(nameof(NodeFactoryCalls))]
        public void Can_create_any_node_with_minimal_arguments(SourceNode node)
        {
            node.Should().NotBeNull();
        }

        public static IEnumerable<object[]> NodeFactoryCalls()
        {
            foreach (var item in NodeFactoryCallsCore())
            {
                yield return new[] { item };
            }
            IEnumerable<SourceNode> NodeFactoryCallsCore()
            {
                var gamesystem = NodeFactory.Gamesystem();
                yield return NodeFactory.Catalogue(gamesystem: gamesystem);

                var catalogue = NodeFactory.Catalogue(gamesystem);
                yield return NodeFactory.CatalogueLink(catalogue: catalogue);

                var categoryEntry = NodeFactory.CategoryEntry();
                yield return NodeFactory.Category(categoryEntry: categoryEntry);
                yield return NodeFactory.CategoryEntry();
                yield return NodeFactory.CategoryLink(categoryEntry: categoryEntry);

                var characteristicType = NodeFactory.CharacteristicType();
                yield return NodeFactory.Characteristic(characteristicType: characteristicType);
                yield return NodeFactory.CharacteristicType();
                yield return NodeFactory.Condition();
                yield return NodeFactory.ConditionGroup(type: ConditionGroupKind.And);
                yield return NodeFactory.Constraint();

                var costType = NodeFactory.CostType();
                yield return NodeFactory.Cost(costType: costType);
                yield return NodeFactory.CostType();
                yield return NodeFactory.Datablob();
                yield return NodeFactory.DataIndex();
                yield return NodeFactory.DataIndexEntry(filePath: "path", node: catalogue);
                yield return NodeFactory.DataIndexRepositoryUrl(value: "url");

                var selectionEntry = NodeFactory.SelectionEntry();
                yield return NodeFactory.EntryLink(selectionEntry: selectionEntry);

                var forceEntry = gamesystem.AddForceEntries(NodeFactory.ForceEntry()).ForceEntries[0];
                yield return NodeFactory.Force(forceEntry: forceEntry);
                yield return NodeFactory.ForceEntry();
                yield return NodeFactory.Gamesystem();
                yield return NodeFactory.InfoGroup();

                var infoGroup = NodeFactory.InfoGroup();
                yield return NodeFactory.InfoLink(infoGroup: infoGroup);
                yield return NodeFactory.Metadata();
                yield return NodeFactory.Modifier();
                yield return NodeFactory.ModifierGroup();

                var profileType = NodeFactory.ProfileType();
                yield return NodeFactory.Profile(profileType: profileType);
                yield return NodeFactory.Publication();
                yield return NodeFactory.Repeat();
                yield return NodeFactory.Roster(gamesystem: gamesystem);
                yield return NodeFactory.Rule();
                yield return NodeFactory.Selection(selectionEntry: selectionEntry, entryId: "entryId");
                yield return NodeFactory.SelectionEntry();
                yield return NodeFactory.SelectionEntryGroup();
            }
        }
    }
}
