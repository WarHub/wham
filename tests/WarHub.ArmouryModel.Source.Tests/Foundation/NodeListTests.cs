﻿using System;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using FluentAssertions;
using Xunit;

namespace WarHub.ArmouryModel.Source.Tests.Foundation
{
    public class NodeListTests
    {
        [Fact]
        public void Slice_supports_ranges()
        {
            var list = CreateList(3);

            var range = list[^2..^1];

            range.Should().BeOfType<NodeList<RuleNode>>();
        }

        [Fact]
        public void CreateFromParams_WithNoArgs_ReturnsDefault()
        {
            var result = NodeList.Create<SourceNode>();
            Assert.StrictEqual(default, result);
        }

        [Fact]
        public void CreateFromParams_WithEmpty_ReturnsDefault()
        {
            var result = NodeList.Create(Array.Empty<SourceNode>());
            Assert.StrictEqual(default, result);
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
            Assert.StrictEqual(default, result);
        }

        [Fact]
        public void CreateFromEnumerable_WithNodes_ReturnsAllPassed()
        {
            var rule1 = CreateRule("1");
            var rule2 = CreateRule("2");
            var result = NodeList.Create<SourceNode>(new[] { rule1, rule2 }.AsEnumerable());
            Assert.Collection(result,
                x => Assert.Same(rule1, x),
                x => Assert.Same(rule2, x));
        }

        [Fact]
        public void NotEquals_SameSequence_InitializedTwice()
        {
            var rule1 = CreateRule("1");
            var rule2 = CreateRule("2");
            var sequence = new[] { rule1, rule2 }.ToList();
            var result1 = NodeList.Create<SourceNode>(sequence);
            var result2 = NodeList.Create<SourceNode>(sequence);
            Assert.NotStrictEqual(result1, result2);
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
            var result2 = NodeList.Create(result1);
            Assert.StrictEqual(result1, result2);
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
            Assert.StrictEqual(result1, result2);
            Assert.False(result1 != result2);
            Assert.True(result1 == result2);
        }

        private static RuleNode CreateRule(string id) =>
            NodeFactory.Rule(id: id);
        private static NodeList<RuleNode> CreateList(int count) =>
            NodeList.Create(Enumerable.Range(1, count).Select(x => CreateRule(x.ToString(CultureInfo.InvariantCulture))));
    }
}
