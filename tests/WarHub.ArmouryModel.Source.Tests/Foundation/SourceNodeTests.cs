using System;
using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
using Xunit;

namespace WarHub.ArmouryModel.Source.Tests.Foundation
{
    public class SourceNodeTests
    {
        [Fact]
        public void NewNode_HasNullParent_and_NullTree()
        {
            var sut = new SimpleTestNode(parent: null);

            sut.Parent.Should().BeNull();
            sut.Tree.Should().BeNull();
        }

        [Fact]
        public void NodeInitializedWithParent_has_NonNullParent()
        {
            var parent = new SimpleTestNode(parent: null);

            var sut = new SimpleTestNode(parent);

            sut.Parent.Should().NotBeNull();
        }

        [Fact]
        public void Creating_SourceTree_with_node_without_core_throws()
        {
            var parent = new SimpleTestNode(parent: null);
            var node = new SimpleTestNode(parent);

            var act = () => SourceTree.CreateForRoot(node);

            act.Should().Throw<InvalidOperationException>().WithMessage("Tree root must be an INodeWithCore*");
        }

        [Fact]
        public void Creating_SourceTree_with_node_without_parent_assigns_tree_property()
        {
            var node = new TestWithCoreNode(new(), parent: null);
            node.Tree.Should().BeNull();

            var tree = SourceTree.CreateForRoot(node);

            node.Tree.Should().BeSameAs(tree);
            tree.GetRoot().Should().BeSameAs(node);
        }

        [Fact]
        public void Creating_SourceTree_with_node_with_parent_creates_tree_with_cloned_node()
        {
            var parent = new TestWithCoreNode(new(), parent: null);
            var node = new TestWithCoreNode(new(), parent);

            var tree = SourceTree.CreateForRoot(node);

            node.Tree.Should().BeNull();
            tree.GetRoot().Should().NotBeSameAs(node);
        }

        [Fact]
        public void Creating_SourceTree_with_node_with_tree_creates_tree_with_cloned_node()
        {
            var node = new TestWithCoreNode(new(), parent: null);
            var treeOld = SourceTree.CreateForRoot(node);
            node.Tree.Should().BeSameAs(treeOld);

            var tree = SourceTree.CreateForRoot(node);

            node.Tree.Should().BeSameAs(treeOld);
            tree.GetRoot().Should().NotBeSameAs(node);
            tree.GetRoot().Tree.Should().BeSameAs(tree);
        }

        private sealed record TestCore : NodeCore, ICore<TestWithCoreNode>
        {
            public override TestWithCoreNode ToNode(SourceNode? parent = null) => new(this, parent);

            protected override int CalculateDescendantSpanLength() => 1;
        }

        private sealed class TestWithCoreNode : SourceNode, INodeWithCore<TestCore>
        {
            public TestWithCoreNode(TestCore core, SourceNode? parent) : base(parent)
            {
                Core = core;
            }

            public TestCore Core { get; }

            public override SourceKind Kind => default;

            public override void Accept(SourceVisitor visitor)
            {
                throw new NotImplementedException();
            }

            [return: MaybeNull]
            public override TResult Accept<TResult>(SourceVisitor<TResult> visitor)
            {
                throw new NotImplementedException();
            }
        }

        private sealed class SimpleTestNode : SourceNode
        {
            public SimpleTestNode(SourceNode? parent) : base(parent)
            {
            }

            public override SourceKind Kind => default;

            public override void Accept(SourceVisitor visitor)
            {
                throw new System.NotImplementedException();
            }

            [return: MaybeNull]
            public override TResult Accept<TResult>(SourceVisitor<TResult> visitor)
            {
                throw new System.NotImplementedException();
            }
        }
    }
}
