using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using System.Reflection;
using FluentAssertions;
using Xunit;
using static WarHub.ArmouryModel.Source.CodeGeneration.Tests.TestHelpers;

namespace WarHub.ArmouryModel.Source.CodeGeneration.Tests
{
    public class GeneratedNodeTests
    {
        public const string ContainerId = "cont1";
        public const string ContainerName = "container name";
        public const string ItemId = "item1";
        public const string ItemName = "item name";

        private static class OneItemContainerPackage
        {
            public static ContainerNode CreateContainer()
            {
                return new ContainerCore.Builder
                {
                    Id = ContainerId,
                    Name = ContainerName,
                    Items =
                {
                    new ItemCore.Builder
                    {
                        Id = ItemId,
                        Name = ItemName
                    }
                }
                }.ToImmutable().ToNode();
            }
        }

        [Theory]
        [InlineData(typeof(ItemNode), nameof(ItemNode.WithName))]
        [InlineData(typeof(ContainerNode), nameof(ContainerNode.WithItems))]
        public void Node_With_parameter_name_is_value(Type type, string withMethodName)
        {
            var parameter = type.GetMethod(withMethodName)!.GetParameters()[0];

            parameter.Name.Should().Be("value");
        }

        [Fact]
        public void ItemNode_HasPublicCoreProperty()
        {
            var core = new ItemCore(null, null).ToNode().Core;

            core.Should().NotBeNull();
        }

        [Fact]
        public void DefaultNodeList_IsEmpty()
        {
            var list = default(NodeList<SourceNode>);
            Assert.Empty(list);
        }

        [Fact]
        public void FactoryMethod_GivenNoItems_CreatesValidNode()
        {
            var container = NodeFactory.Container(ContainerId, ContainerName);

            Assert.Empty(container.Items);
        }

        [Fact]
        public void Deconstruct_ReturnsValidValues()
        {
            var container = OneItemContainerPackage.CreateContainer();

            container.Deconstruct(out var id, out var name, out var items);
            Assert.Equal(ContainerId, id);
            Assert.Equal(ContainerName, name);
            Assert.Collection(
                items,
                x =>
                {
                    Assert.Equal(ItemId, x.Id);
                    Assert.Equal(ItemName, x.Name);
                });
        }

        [Fact]
        public void With_collection_has_no_overloads()
        {
            Action act = () => typeof(ContainerNode).GetMethod(nameof(ContainerNode.WithItems));

            act.Should().NotThrow<AmbiguousMatchException>();
        }

        [Fact]
        public void With_CollectionProperty_ParamsOverride_ReplacesItems()
        {
            const string Item1Id = "id1";
            const string Item2Id = "id2";
            var item1 = new ItemCore.Builder { Id = Item1Id };
            var item2Node = new ItemCore.Builder { Id = Item2Id }.ToImmutable().ToNode();
            var container = new ContainerCore.Builder { Items = { item1 } }.ToImmutable().ToNode();
            var newContainer = container.WithItems(item2Node);
            Assert.NotSame(container, newContainer);
            Assert.Collection(
                container.Items,
                x => Assert.Equal(Item1Id, x.Id));
            Assert.Collection(
                newContainer.Items,
                x => Assert.Equal(Item2Id, x.Id));
        }

        [Fact]
        public void Add_CollectionProperty_ParamsOverride_AddsItems()
        {
            const string Item1Id = "id1";
            const string Item2Id = "id2";
            var item1 = new ItemCore.Builder { Id = Item1Id };
            var item2Node = new ItemCore.Builder { Id = Item2Id }.ToImmutable().ToNode();
            var container = new ContainerCore.Builder { Items = { item1 } }.ToImmutable().ToNode();
            var newContainer = container.AddItems(item2Node);
            Assert.NotSame(container, newContainer);
            Assert.Collection(
                container.Items,
                x => Assert.Equal(Item1Id, x.Id));
            Assert.Collection(
                newContainer.Items,
                x => Assert.Equal(Item1Id, x.Id),
                x => Assert.Equal(Item2Id, x.Id));
        }

        [Fact]
        public void Add_CollectionProperty_IEnumerableOverride_AddsItems()
        {
            const string Item1Id = "id1";
            const string Item2Id = "id2";
            var item1 = new ItemCore.Builder { Id = Item1Id };
            var item2Node = new ItemCore.Builder { Id = Item2Id }.ToImmutable().ToNode();
            var container = new ContainerCore.Builder { Items = { item1 } }.ToImmutable().ToNode();
            var newContainer = container.AddItems(new[] { item2Node }.ToImmutableList());
            Assert.NotSame(container, newContainer);
            Assert.Collection(
                container.Items,
                x => Assert.Equal(Item1Id, x.Id));
            Assert.Collection(
                newContainer.Items,
                x => Assert.Equal(Item1Id, x.Id),
                x => Assert.Equal(Item2Id, x.Id));
        }

        [Fact]
        public void Ancestors_OnRoot_ReturnsNone()
        {
            var item = new ItemCore.Builder().ToImmutable().ToNode();
            var ancestors = item.Ancestors();
            Assert.Empty(ancestors);
        }

        [Fact]
        public void Ancestors_OnChild_ReturnsParent()
        {
            var container = OneItemContainerPackage.CreateContainer();
            var ancestors = container.Items[0].Ancestors();
            Assert.Collection(ancestors,
                x => Assert.Same(container.Items, x),
                x => Assert.Same(container, x));
        }

        [Fact]
        public void Ancestors_OnGrandchild_ReturnsRoot()
        {
            var root = EmptyRootNode.AddLeftContainers(OneItemContainerPackage.CreateContainer());
            var container = root.LeftContainers[0];
            var ancestors = container.Items[0].Ancestors();
            Assert.Collection(ancestors,
                x => Assert.Same(container.Items, x),
                x => Assert.Same(container, x),
                x => Assert.Same(root.LeftContainers, x),
                x => Assert.Same(root, x));
        }

        [Fact]
        public void AncestorsAndSelf_OnRoot_ReturnsSelf()
        {
            var item = new ItemCore.Builder().ToImmutable().ToNode();
            var ancestors = item.AncestorsAndSelf();
            Assert.Collection(ancestors, x => Assert.Same(item, x));
        }

        [Fact]
        public void AncestorsAndSelf_OnChild_ReturnsParent()
        {
            var container = OneItemContainerPackage.CreateContainer();
            var item = container.Items[0];
            var ancestors = item.AncestorsAndSelf();
            Assert.Collection(ancestors,
                x => Assert.Same(item, x),
                x => Assert.Same(container.Items, x),
                x => Assert.Same(container, x));
        }

        [Fact]
        public void AncestorsAndSelf_OnGrandchild_ReturnsRoot()
        {
            var root = EmptyRootNode.AddLeftContainers(OneItemContainerPackage.CreateContainer());
            var container = root.LeftContainers[0];
            var item = container.Items[0];
            var ancestors = item.AncestorsAndSelf();
            Assert.Collection(ancestors,
                x => Assert.Same(item, x),
                x => Assert.Same(container.Items, x),
                x => Assert.Same(container, x),
                x => Assert.Same(root.LeftContainers, x),
                x => Assert.Same(root, x));
        }

        [Fact]
        public void Children_OnRoot_ReturnsOnlyDirectChildren()
        {
            const string Container1Id = "container1";
            const string Container2Id = "container2";
            var root = EmptyRootNode
                .AddLeftContainers(OneItemContainerPackage.CreateContainer().WithId(Container1Id))
                .AddRightContainers(OneItemContainerPackage.CreateContainer().WithId(Container2Id));
            var children = root.Children();

            Assert.Collection(children,
                x => Assert.IsType<ContainerListNode>(x),
                x => Assert.IsType<ContainerListNode>(x));

            Assert.Collection((ContainerListNode)children.ElementAt(0),
                x => Assert.True(x is ContainerNode c && c.Id == Container1Id));

            Assert.Collection((ContainerListNode)children.ElementAt(1),
                x => Assert.True(x is ContainerNode c && c.Id == Container2Id));
        }

        [Fact]
        public void Children_OfList_ReturnsChildren()
        {
            var container =
                NodeFactory.Container("id", "container")
                .AddItems(
                    NodeFactory.Item("child1", ""),
                    NodeFactory.Item("child2", ""));
            var list = container.Items;

            Assert.Collection(
                list.Children().Cast<ItemNode>(),
                x => Assert.Same(list[0], x),
                x => Assert.Same(list[1], x));
        }

        [Fact]
        public void ChildrenInfos_OfList_ReturnsChildrenInfos()
        {
            const string Child1Id = "child1";
            const string Child2Id = "child2";
            var container =
                NodeFactory.Container("id", "container")
                .AddItems(
                    NodeFactory.Item(Child1Id, ""),
                    NodeFactory.Item(Child2Id, ""));
            var list = container.Items;

            Assert.Collection(
                list.ChildrenInfos().Select((x, i) => KeyValuePair.Create(i, x)),
                AssertChild,
                AssertChild);

            void AssertChild(KeyValuePair<int, ChildInfo> pair)
            {
                Assert.Equal(pair.Key.ToString("D", CultureInfo.InvariantCulture), pair.Value.Name);
                Assert.Same(list[pair.Key], pair.Value.Node);
            }
        }

        [Fact]
        public void Contains_Grandchild_ReturnsTrue()
        {
            var root = EmptyRootNode.AddLeftContainers(OneItemContainerPackage.CreateContainer());
            var item = root.LeftContainers[0].Items[0];
            var contains = root.Contains(item);
            Assert.True(contains);
        }

        [Fact]
        public void Contains_Child_ReturnsFalse()
        {
            var root = EmptyRootNode.AddLeftContainers(OneItemContainerPackage.CreateContainer());
            var otherContainer = root.LeftContainers[0].WithId("other");
            var contains = root.Contains(otherContainer);
            Assert.False(contains);
        }

        [Fact]
        public void Descendants_OnLeaf_ReturnsNone()
        {
            var leaf = EmptyItemNode;
            var descendants = leaf.Descendants();
            Assert.Empty(descendants);
        }

        [Fact]
        public void Descendants_OnTreeRoot_ReturnsAllExceptRoot()
        {
            var root = SymmetricTree.Create();
            var descendants = root.Descendants();
            Assert.Collection(descendants, SymmetricTree.ValidateElements(includeSelf: false).ToArray());
        }

        [Fact]
        public void Descendants_OnTreeRoot_WithNoContainerChildrenPredicate_ReturnsAllContainers()
        {
            var root = SymmetricTree.Create();
            var descendants = root.Descendants(descendIntoChildren: x => !(x is ContainerNode));
            Assert.Collection(
                descendants,
                x => Assert.IsType<ContainerListNode>(x),
                x => Assert.IsType<ContainerNode>(x),
                x => Assert.IsType<ContainerNode>(x),
                x => Assert.IsType<ContainerListNode>(x),
                x => Assert.IsType<ContainerNode>(x),
                x => Assert.IsType<ContainerNode>(x));
        }

        [Fact]
        public void DescendantsAndSelf_OnLeaf_ReturnsSelf()
        {
            var leaf = EmptyItemNode;
            var descendants = leaf.DescendantsAndSelf();
            Assert.Collection(descendants, x => Assert.Same(leaf, x));
        }

        [Fact]
        public void DescendantsAndSelf_OnTreeRoot_ReturnsAll()
        {
            var root = SymmetricTree.Create();
            var descendants = root.DescendantsAndSelf();
            Assert.Collection(descendants, SymmetricTree.ValidateElements(includeSelf: true).ToArray());
        }

        [Fact]
        public void DescendantsAndSelf_OnTreeRoot_WithNoContainerChildrenPredicate_ReturnsAllContainersAndRoot()
        {
            var root = SymmetricTree.Create();
            var descendants = root.DescendantsAndSelf(descendIntoChildren: x => !(x is ContainerNode));
            Assert.Collection(
                descendants,
                x => Assert.IsType<RootContainerNode>(x),
                x => Assert.IsType<ContainerListNode>(x),
                x => Assert.IsType<ContainerNode>(x),
                x => Assert.IsType<ContainerNode>(x),
                x => Assert.IsType<ContainerListNode>(x),
                x => Assert.IsType<ContainerNode>(x),
                x => Assert.IsType<ContainerNode>(x));
        }

        [Fact]
        public void FirstAncestorOrSelf_OnItem_OfTypeContainer_ReturnsContainer()
        {
            var container = OneItemContainerPackage.CreateContainer();
            var item = container.Items[0];
            var ancestor = item.FirstAncestorOrSelf<ContainerNode>(_ => true);
            Assert.Same(container, ancestor);
        }

        [Fact]
        public void FirstAncestorOrSelf_OnItem_OfTypeContainer_ReturnsNull()
        {
            var container = OneItemContainerPackage.CreateContainer();
            var item = container.Items[0];
            var ancestor = item.FirstAncestorOrSelf<ContainerNode>(_ => false);
            Assert.Null(ancestor);
        }

        [Fact]
        public void FirstAncestorOrSelf_OnTreeItem_OfTypeRoot_ReturnsRoot()
        {
            var root = EmptyRootNode.AddLeftContainers(EmptyContainerNode.AddItems(EmptyItemNode));
            var item = root.LeftContainers[0].Items[0];
            var ancestor = item.FirstAncestorOrSelf<RootContainerNode>(_ => true);
            Assert.Same(root, ancestor);
        }

        [Fact]
        public void FirstAncestorOrSelf_OnTreeItem_OfTypeRoot_ReturnsNull()
        {
            var root = EmptyRootNode.AddLeftContainers(EmptyContainerNode.AddItems(EmptyItemNode));
            var item = root.LeftContainers[0].Items[0];
            var ancestor = item.FirstAncestorOrSelf<RootContainerNode>(_ => false);
            Assert.Null(ancestor);
        }

        [Fact]
        public void FirstAncestorOrSelf_OnRecursiveTreeItem_GivenContainerName_FindsContainer()
        {
            const string RootName = "root";
            const string Name1 = "container1";
            const string Name2 = "container2";
            const string Name3 = "container3";
            const string Name4 = "container4";
            var container = EmptyRecursiveNode;
            var root = EmptyRecursiveNode.WithName(RootName)
                .AddContainers(
                    container.WithName(Name1)
                    .AddContainers(
                        container.WithName(Name2)
                        .AddContainers(
                            container.WithName(Name3)
                            .AddContainers(
                                container.WithName(Name4)
                                .AddItems(EmptyItemNode)))));
            var item = root.Descendants().OfType<ItemNode>().Single();
            var expected = root.Containers[0].Containers[0];
            // act
            var actual = item.FirstAncestorOrSelf<RecursiveContainerNode>(x => x.Name == Name2);
            Assert.Same(expected, actual);
        }

        [Fact]
        public void With_when_value_equals_old_returns_this_instance()
        {
            var subject = NodeFactory.Container("id", "name", NodeFactory.ItemList(NodeFactory.Item("a", "b")));
            var result = subject
                .WithId("id")
                .WithName("name")
                .WithItems(subject.Items.NodeList);
            result.Should().BeSameAs(subject);
        }

        [Fact]
        public void WithNodes_when_NodeList_equals_old_returns_this_instance()
        {
            var subject = NodeFactory.ItemList(
                NodeFactory.Item("1", "a1"),
                NodeFactory.Item("2", "a2"));
            var result = subject.WithNodes(subject.NodeList);
            result.Should().BeSameAs(subject);
        }
    }
}
