using System;
using System.Collections.Generic;
using WarHub.ArmouryModel.Source.CodeGeneration.Tests.GeneratedCode;
using Xunit;

namespace WarHub.ArmouryModel.Source.CodeGeneration.Tests
{
    public static class TestHelpers
    {
        public static RootContainerNode EmptyRootNode
            => new RootContainerCore.Builder().ToImmutable().ToNode();

        public static ItemNode EmptyItemNode
            => new ItemCore.Builder().ToImmutable().ToNode();

        public static ContainerNode EmptyContainerNode
            => new ContainerCore.Builder().ToImmutable().ToNode();

        public static RecursiveContainerNode EmptyRecursiveNode
            => new RecursiveContainerCore.Builder().ToImmutable().ToNode();

        public static class SymmetricTree
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
