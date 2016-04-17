// WarHub licenses this file to you under the MIT license.
// See LICENSE file in the project root for more information.

namespace WarHub.Armoury.Model.EntryTreeTests
{
    using System.Collections.Generic;
    using System.Linq;
    using EntryTree;
    using NSubstitute;
    using Xunit;

    public class NodeExtensionsTests
    {
        [Fact]
        public void DescendantLinkNodes_Empty_Success()
        {
            var actingNode = new NodeSubBuilder().BuildNode();

            var resultNodes = actingNode.DescendantLinkNodes();

            Assert.Equal(0, resultNodes.Count());
        }

        [Fact]
        public void DescendantLinkNodes_ReturnsChildrenOfNonLinkNode_Success()
        {
            var actingNode = new NodeSubBuilder
            {
                Child = new NodeSubBuilder
                {
                    Child = new NodeSubBuilder {IsLinkNode = true}
                }
            }.BuildNode();
            var linkNode = actingNode.Children.First().Children.First();

            var resultNodes = actingNode.DescendantLinkNodes();

            Assert.Same(linkNode, resultNodes.Single());
        }

        [Fact]
        public void DescendantLinkNodes_ReturnsLinkNode_Success()
        {
            var actingNode = new NodeSubBuilder
            {
                Child = new NodeSubBuilder {IsLinkNode = true}
            }.BuildNode();
            var linkNode = actingNode.Children.First();

            var resultNodes = actingNode.DescendantLinkNodes();

            Assert.Same(linkNode, resultNodes.Single());
        }

        [Fact]
        public void DescendantNotLinkGroupNodes_Empty_Success()
        {
            var actingNode = new GroupNodeSubBuilder
            {
                Child = new GroupNodeSubBuilder {IsLinkNode = true}
            }.BuildNode();

            var resultNodes = actingNode.DescendantNotLinkGroupNodes().ToList();

            Assert.Equal(0, resultNodes.Count);
        }

        [Fact]
        public void DescendantNotLinkGroupNodes_ReturnsNoLinked_Success()
        {
            var actingNode = new GroupNodeSubBuilder
            {
                Children = new List<GroupNodeSubBuilder>
                {
                    new GroupNodeSubBuilder(),
                    new GroupNodeSubBuilder {IsLinkNode = true}
                }
            }.BuildNode();
            var notLinkNode = actingNode.GroupNodes.Single(node => !node.IsLinkNode);

            var resultNodes = actingNode.DescendantNotLinkGroupNodes();

            Assert.Same(notLinkNode, resultNodes.Single());
        }

        [Fact]
        public void DescendantNotLinkGroupNodes_ReturnsNotLinkedGrandChildren_Success()
        {
            var actingNode = new GroupNodeSubBuilder
            {
                Children = new List<GroupNodeSubBuilder>
                {
                    new GroupNodeSubBuilder {IsLinkNode = true},
                    new GroupNodeSubBuilder
                    {
                        Child = new GroupNodeSubBuilder()
                    }
                }
            }.BuildNode();
            var notLinkNode = actingNode.GroupNodes.Single(node => !node.IsLinkNode);
            var grandChildNode = notLinkNode.GroupNodes.Single();

            var resultNodes = actingNode.DescendantNotLinkGroupNodes().ToList();

            Assert.Equal(2, resultNodes.Count);
            Assert.True(resultNodes.Contains(grandChildNode));
            Assert.True(resultNodes.Contains(notLinkNode));
        }

        [Fact]
        public void Parents_RootGivesEmpty_Success()
        {
            var root = Substitute.For<INode>();
            root.IsRoot.Returns(true);

            var parents = root.Parents().ToList();

            Assert.Equal(0, parents.Count);
        }

        [Fact]
        public void Parents_SingleParent_Success()
        {
            var root = Substitute.For<INode>();
            root.IsRoot.Returns(true);
            var node = Substitute.For<INode>();
            node.Parent.Returns(root);

            var parents = node.Parents().ToList();

            Assert.Equal(1, parents.Count);
            Assert.Same(root, parents[0]);
        }

        public class NodeSubBuilder
        {
            public NodeSubBuilder Child { get; set; }

            public bool IsLinkNode { get; set; }

            public INode BuildNode()
            {
                var node = Substitute.For<INode>();
                node.IsLinkNode.Returns(IsLinkNode);

                var children =
                    (Child != null ? new List<NodeSubBuilder> {Child} : new List<NodeSubBuilder>(0)).Select(
                        builder => builder.BuildNode()).ToList();
                node.Children.Returns(children);

                return node;
            }
        }

        public class GroupNodeSubBuilder
        {
            public GroupNodeSubBuilder Child { get; set; }

            public List<GroupNodeSubBuilder> Children { get; set; }

            public bool IsLinkNode { get; set; }

            public IGroupNode BuildNode()
            {
                var node = Substitute.For<IGroupNode>();
                node.IsLinkNode.Returns(IsLinkNode);
                if (Children == null)
                {
                    var children =
                        (Child != null ? new List<GroupNodeSubBuilder> {Child} : new List<GroupNodeSubBuilder>(0))
                            .Select(
                                builder => builder.BuildNode()).ToList();
                    node.GroupNodes.Returns(children);
                }
                else
                {
                    var children = Children.Select(builder => builder.BuildNode()).ToList();
                    node.GroupNodes.Returns(children);
                }

                return node;
            }
        }
    }
}
