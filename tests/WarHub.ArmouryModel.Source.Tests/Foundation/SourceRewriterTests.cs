using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Xunit;
using static WarHub.ArmouryModel.Source.NodeFactory;

namespace WarHub.ArmouryModel.Source.Tests.Foundation
{
    public class SourceRewriterTests
    {
        [Theory]
        [MemberData(nameof(Sources))]
        public void IdentityRewriter_returns_the_same_instance(SourceNode root)
        {
            var rewriter = new IdentityRewriter();

            var result = rewriter.Visit(root);

            result.Should().BeSameAs(root);
        }

        [Fact]
        public void Rewriter_that_changes_node_returns_new_tree()
        {
            var subject = InfoGroup().AddRules(Rule("rule1"), Rule("rule2"));
            var result = LambdaRewriter.Visit(subject, node => node switch
                {
                    RuleNode r => r.WithName(r.Name + r.Name),
                    _ => node
                });
            result.Rules.Should()
                .SatisfyRespectively(
                    first => first.Name.Should().Be("rule1rule1"),
                    first => first.Name.Should().Be("rule2rule2"));
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        public void Rewriter_VisitNode_that_returns_null_removes_visited_node_from_tree(int removedRuleIndex)
        {
            var subject = InfoGroup().AddRules(Rule("rule1"), Rule("rule2"));
            var result =
                LambdaRewriter.Visit(
                    subject,
                    node => node is RuleNode && node.IndexInParent == removedRuleIndex ? null : node);
            result.Rules.Should()
                .HaveCount(1)
                .And
                .SatisfyRespectively(first => first.Name.Should().NotBe(subject.Rules[removedRuleIndex].Name));
        }

        public static IEnumerable<object[]> Sources()
        {
            return from node in _() select new object[] { node };

            static IEnumerable<SourceNode> _()
            {
                var gst = Gamesystem();
                yield return gst;
                yield return Catalogue(gst).AddPublications(Publication("Test"));
            }
        }
    }

    internal class IdentityRewriter : SourceRewriter
    {
    }

    internal class LambdaRewriter : SourceRewriter
    {
        private readonly Func<SourceNode, SourceNode> selector;

        public LambdaRewriter(Func<SourceNode, SourceNode> selector)
        {
            this.selector = selector;
        }

        public static TNode Visit<TNode>(TNode node, Func<SourceNode, SourceNode> selector)
            where TNode : SourceNode
        {
            var visitor = new LambdaRewriter(selector);
            return (TNode)visitor.Visit(node);
        }

        public override SourceNode Visit(SourceNode node)
        {
            return base.Visit(selector(node));
        }
    }
}
