using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using WarHub.ArmouryModel.Source.CodeGeneration.Tests.GeneratedCode;
using Xunit;

namespace WarHub.ArmouryModel.Source.CodeGeneration.Tests
{
    public class GeneratedNodeTests
    {
        public const string containerId = "cont1";
        public const string containerName = "container name";
        public const string itemId = "item1";
        public const string itemName = "item name";

        public class OneItemContainerPackage
        {

            public static ContainerNode CreateContainer()
            {
                return new ContainerCore.Builder
                {
                    Id = containerId,
                    Name = containerName,
                    Items =
                {
                    new ItemCore.Builder
                    {
                        Id = itemId,
                        Name = itemName
                    }
                }
                }.ToImmutable().ToNode();
            }
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
            var container = GeneratedCode.NodeFactory.Container(containerId, containerName);

            Assert.Empty(container.Items);
        }

        [Fact]
        public void Deconstruct_ReturnsValidValues()
        {
            var container = OneItemContainerPackage.CreateContainer();

            container.Deconstruct(out var id, out var name, out var items);
            Assert.Equal(containerId, id);
            Assert.Equal(containerName, name);
            Assert.Collection(
                items,
                x =>
                {
                    Assert.Equal(itemId, x.Id);
                    Assert.Equal(itemName, x.Name);
                });
        }

        [Fact]
        public void With_CollectionProperty_ParamsOverride_ReplacesItems()
        {
            const string item1Id = "id1";
            const string item2Id = "id2";
            var item1 = new ItemCore.Builder { Id = item1Id };
            var item2Node = new ItemCore.Builder { Id = item2Id }.ToImmutable().ToNode();
            var container = new ContainerCore.Builder { Items = { item1 } }.ToImmutable().ToNode();
            var newContainer = container.WithItems(item2Node);
            Assert.NotSame(container, newContainer);
            Assert.Collection(
                container.Items,
                x => Assert.Equal(item1Id, x.Id));
            Assert.Collection(
                newContainer.Items,
                x => Assert.Equal(item2Id, x.Id));
        }

        [Fact]
        public void Add_CollectionProperty_ParamsOverride_AddsItems()
        {
            const string item1Id = "id1";
            const string item2Id = "id2";
            var item1 = new ItemCore.Builder { Id = item1Id };
            var item2Node = new ItemCore.Builder { Id = item2Id }.ToImmutable().ToNode();
            var container = new ContainerCore.Builder { Items = { item1 } }.ToImmutable().ToNode();
            var newContainer = container.AddItems(item2Node);
            Assert.NotSame(container, newContainer);
            Assert.Collection(
                container.Items,
                x => Assert.Equal(item1Id, x.Id));
            Assert.Collection(
                newContainer.Items,
                x => Assert.Equal(item1Id, x.Id),
                x => Assert.Equal(item2Id, x.Id));
        }

        [Fact]
        public void Add_CollectionProperty_IEnumerableOverride_AddsItems()
        {
            const string item1Id = "id1";
            const string item2Id = "id2";
            var item1 = new ItemCore.Builder { Id = item1Id };
            var item2Node = new ItemCore.Builder { Id = item2Id }.ToImmutable().ToNode();
            var container = new ContainerCore.Builder { Items = { item1 } }.ToImmutable().ToNode();
            var newContainer = container.AddItems(new[] { item2Node }.ToImmutableList());
            Assert.NotSame(container, newContainer);
            Assert.Collection(
                container.Items,
                x => Assert.Equal(item1Id, x.Id));
            Assert.Collection(
                newContainer.Items,
                x => Assert.Equal(item1Id, x.Id),
                x => Assert.Equal(item2Id, x.Id));
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
            Assert.Collection(ancestors, x => Assert.Same(container, x));
        }

        [Fact]
        public void Ancestors_OnGrandchild_ReturnsRoot()
        {
            var root = EmptyRootNode.AddLeftContainers(OneItemContainerPackage.CreateContainer());
            var container = root.LeftContainers[0];
            var ancestors = container.Items[0].Ancestors();
            Assert.Collection(ancestors, x => Assert.Same(container, x), x => Assert.Same(root, x));
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
            Assert.Collection(ancestors, x => Assert.Same(item, x), x => Assert.Same(container, x));
        }

        [Fact]
        public void AncestorsAndSelf_OnGrandchild_ReturnsRoot()
        {
            var root = EmptyRootNode.AddLeftContainers(OneItemContainerPackage.CreateContainer());
            var container = root.LeftContainers[0];
            var item = container.Items[0];
            var ancestors = item.AncestorsAndSelf();
            Assert.Collection(ancestors, x => Assert.Same(item, x), x => Assert.Same(container, x), x => Assert.Same(root, x));
        }

        [Fact]
        public void Children_OnRoot_ReturnsOnlyDirectChildren()
        {
            const string container1Id = "container1";
            const string container2Id = "container2";
            var root = EmptyRootNode
                .AddLeftContainers(OneItemContainerPackage.CreateContainer().WithId(container1Id))
                .AddRightContainers(OneItemContainerPackage.CreateContainer().WithId(container2Id));
            var children = root.Children();
            Assert.Collection(children,
                x => Assert.True(x is ContainerNode c && c.Id == container1Id),
                x => Assert.True(x is ContainerNode c && c.Id == container2Id));
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
                x => Assert.IsType<ContainerNode>(x),
                x => Assert.IsType<ContainerNode>(x),
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
                x => Assert.IsType<ContainerNode>(x),
                x => Assert.IsType<ContainerNode>(x),
                x => Assert.IsType<ContainerNode>(x),
                x => Assert.IsType<ContainerNode>(x));
        }

        [Fact]
        public void FirstAncestorOrSelf_OnItem_OfTypeContainer_ReturnsContainer()
        {
            var container = OneItemContainerPackage.CreateContainer();
            var item = container.Items[0];
            var ancestor = item.FirstAncestorOrSelf<ContainerNode>(x => true);
            Assert.Same(container, ancestor);
        }

        [Fact]
        public void FirstAncestorOrSelf_OnItem_OfTypeContainer_ReturnsNull()
        {
            var container = OneItemContainerPackage.CreateContainer();
            var item = container.Items[0];
            var ancestor = item.FirstAncestorOrSelf<ContainerNode>(x => false);
            Assert.Null(ancestor);
        }

        [Fact]
        public void FirstAncestorOrSelf_OnTreeItem_OfTypeRoot_ReturnsRoot()
        {
            var root = EmptyRootNode.AddLeftContainers(EmptyContainerNode.AddItems(EmptyItemNode));
            var item = root.LeftContainers[0].Items[0];
            var ancestor = item.FirstAncestorOrSelf<RootContainerNode>(x => true);
            Assert.Same(root, ancestor);
        }

        [Fact]
        public void FirstAncestorOrSelf_OnTreeItem_OfTypeRoot_ReturnsNull()
        {
            var root = EmptyRootNode.AddLeftContainers(EmptyContainerNode.AddItems(EmptyItemNode));
            var item = root.LeftContainers[0].Items[0];
            var ancestor = item.FirstAncestorOrSelf<RootContainerNode>(x => false);
            Assert.Null(ancestor);
        }

        [Fact]
        public void FirstAncestorOrSelf_OnRecursiveTreeItem_GivenContainerName_FindsContainer()
        {
            const string rootName = "root";
            const string name1 = "container1";
            const string name2 = "container2";
            const string name3 = "container3";
            const string name4 = "container4";
            var container = EmptyRecursiveNode;
            var root = EmptyRecursiveNode.WithName(rootName)
                .AddContainers(
                    container.WithName(name1)
                    .AddContainers(
                        container.WithName(name2)
                        .AddContainers(
                            container.WithName(name3)
                            .AddContainers(
                                container.WithName(name4)
                                .AddItems(EmptyItemNode)))));
            var item = root.Descendants().OfType<ItemNode>().Single();
            var expected = root.Containers[0].Containers[0];
            // act
            var actual = item.FirstAncestorOrSelf<RecursiveContainerNode>(x => x.Name == name2);
            Assert.Same(expected, actual);
        }

        private static RootContainerNode EmptyRootNode
            => new RootContainerCore.Builder().ToImmutable().ToNode();

        private static ItemNode EmptyItemNode
            => new ItemCore.Builder().ToImmutable().ToNode();

        private static ContainerNode EmptyContainerNode
            => new ContainerCore.Builder().ToImmutable().ToNode();

        private static RecursiveContainerNode EmptyRecursiveNode
            => new RecursiveContainerCore.Builder().ToImmutable().ToNode();

        private class SymmetricTree
        {
            public static RootContainerNode Create()
            {
                var leaf = EmptyItemNode;
                var container = EmptyContainerNode;
                var root = EmptyRootNode
                    .AddLeftContainers(
                        container.AddItems(leaf, leaf),
                        container.AddItems(leaf))
                    .AddRightContainers(
                        container.AddItems(leaf),
                        container.AddItems(leaf, leaf));
                return root;
            }

            public static IEnumerable<Action<SourceNode>> ValidateElements(bool includeSelf)
            {
                if (includeSelf)
                {
                    yield return x => Assert.IsType<RootContainerNode>(x);
                }
                yield return x => Assert.IsType<ContainerNode>(x);
                yield return x => Assert.IsType<ItemNode>(x);
                yield return x => Assert.IsType<ItemNode>(x);
                yield return x => Assert.IsType<ContainerNode>(x);
                yield return x => Assert.IsType<ItemNode>(x);
                yield return x => Assert.IsType<ContainerNode>(x);
                yield return x => Assert.IsType<ItemNode>(x);
                yield return x => Assert.IsType<ContainerNode>(x);
                yield return x => Assert.IsType<ItemNode>(x);
                yield return x => Assert.IsType<ItemNode>(x);
            }
        }
    }
}
