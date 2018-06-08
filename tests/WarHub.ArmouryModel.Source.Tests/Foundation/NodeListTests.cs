using System.Collections.Immutable;
using System.Linq;
using Xunit;

namespace WarHub.ArmouryModel.Source.Tests.Foundation
{
    public class NodeListTests
    {
        [Fact]
        public void CreateFromParams_WithNoArgs_ReturnsDefault()
        {
            var result = NodeList.Create<SourceNode>();
            Assert.Equal(default, result);
        }

        [Fact]
        public void CreateFromParams_WithEmpty_ReturnsDefault()
        {
            var result = NodeList.Create(new SourceNode[0]);
            Assert.Equal(default, result);
        }

        [Fact]
        public void CreateFromParams_WithNodes_ReturnsAllPassed()
        {
            var rule1 = CreateRule("1");
            var rule2 = CreateRule("2");
            var result = NodeList.Create<SourceNode>(rule1, rule2);
            Assert.Collection(result,
                x => Assert.Same(rule1, x),
                x => Assert.Same(rule2, x));
        }

        [Fact]
        public void CreateFromEnumerable_WithEmpty_ReturnsDefault()
        {
            var result = NodeList.Create(Enumerable.Empty<SourceNode>());
            Assert.Equal(default, result);
        }

        [Fact]
        public void CreateFromEnumerable_WithNodes_ReturnsAllPassed()
        {
            var rule1 = CreateRule("1");
            var rule2 = CreateRule("2");
            var result = NodeList.Create<SourceNode>(new[]{rule1, rule2}.AsEnumerable());
            Assert.Collection(result,
                x => Assert.Same(rule1, x),
                x => Assert.Same(rule2, x));
        }

        [Fact]
        public void NotEquals_SameSequence_InitializedTwice()
        {
            var rule1 = CreateRule("1");
            var rule2 = CreateRule("2");
            var sequence = new[] {rule1, rule2}.ToList();
            var result1 = NodeList.Create<SourceNode>(sequence);
            var result2 = NodeList.Create<SourceNode>(sequence);
            Assert.NotEqual(result1, result2);
            Assert.True(result1 != result2);
            Assert.False(result1 == result2);
        }

        [Fact]
        public void Equals_SameSequence_InitializedFromTheOther()
        {
            var rule1 = CreateRule("1");
            var rule2 = CreateRule("2");
            var sequence = new[] { rule1, rule2 }.ToList();
            var result1 = NodeList.Create<SourceNode>(sequence);
            var result2 = NodeList.Create<SourceNode>(result1);
            Assert.Equal(result1, result2);
            Assert.False(result1 != result2);
            Assert.True(result1 == result2);
        }

        [Fact]
        public void Equals_SameSequence_InitializedFromImmutableArray()
        {
            var rule1 = CreateRule("1");
            var rule2 = CreateRule("2");
            var immutableArray = new[] { rule1, rule2 }.ToImmutableArray();
            var result1 = NodeList.Create<SourceNode>(immutableArray);
            var result2 = NodeList.Create<SourceNode>(immutableArray);
            Assert.Equal(result1, result2);
            Assert.False(result1 != result2);
            Assert.True(result1 == result2);
        }

        private static RuleNode CreateRule(string id) =>
            NodeFactory.Rule(id, default, default, default, default, default);
    }
}
