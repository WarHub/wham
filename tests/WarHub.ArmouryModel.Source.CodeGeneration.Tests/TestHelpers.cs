using System;
using System.Collections.Generic;
using Xunit;

namespace WarHub.ArmouryModel.Source.CodeGeneration.Tests
{
    internal static class TestHelpers
    {
        public static RootContainerNode EmptyRootNode
            => RootContainerCore.Empty.ToNode();

        public static ItemNode EmptyItemNode
            => ItemCore.Empty.ToNode();

        public static ContainerNode EmptyContainerNode
            => ContainerCore.Empty.ToNode();

        public static RecursiveContainerNode EmptyRecursiveNode
            => RecursiveContainerCore.Empty.ToNode();

        public static class SymmetricTree
        {
            public static RootContainerNode Create()
            {
                var leaf = EmptyItemNode;
                var container = EmptyContainerNode;
                return EmptyRootNode
                    .AddLeftContainers(
                        container.AddItems(leaf, leaf),
                        container.AddItems(leaf))
                    .AddRightContainers(
                        container.AddItems(leaf),
                        container.AddItems(leaf, leaf));
            }

            public static IEnumerable<Action<SourceNode>> ValidateElements(bool includeSelf)
            {
                if (includeSelf)
                {
                    yield return x => Assert.IsType<RootContainerNode>(x);
                }
                yield return x => Assert.IsType<ContainerListNode>(x);
                yield return x => Assert.IsType<ContainerNode>(x);
                yield return x => Assert.IsType<ItemListNode>(x);
                yield return x => Assert.IsType<ItemNode>(x);
                yield return x => Assert.IsType<ItemNode>(x);
                yield return x => Assert.IsType<ContainerNode>(x);
                yield return x => Assert.IsType<ItemListNode>(x);
                yield return x => Assert.IsType<ItemNode>(x);
                yield return x => Assert.IsType<ContainerListNode>(x);
                yield return x => Assert.IsType<ContainerNode>(x);
                yield return x => Assert.IsType<ItemListNode>(x);
                yield return x => Assert.IsType<ItemNode>(x);
                yield return x => Assert.IsType<ContainerNode>(x);
                yield return x => Assert.IsType<ItemListNode>(x);
                yield return x => Assert.IsType<ItemNode>(x);
                yield return x => Assert.IsType<ItemNode>(x);
            }
        }
    }
}
