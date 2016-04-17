// WarHub licenses this file to you under the MIT license.
// See LICENSE file in the project root for more information.

namespace WarHub.Armoury.Model.ConditionResolversTests
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using Builders;
    using ConditionResolvers;
    using NSubstitute;
    using Xunit;

    public class CategoryChildValueExtractorTest
    {
        [Fact]
        public void GameSystemCondition_CategoryPercent_Success()
        {
            const decimal pointsTotal = 678;
            const decimal pointsLimit = 1000;
            var builder = CreateCategoryBuilder();

            var roster = builder.AncestorContext.RosterBuilder.Roster;
            roster.PointsLimit.Returns(pointsLimit);

            var stats = Substitute.For<IStatAggregate>();
            stats.PointsTotal.Returns(pointsTotal);
            builder.StatAggregate.Returns(stats);

            var categoryGuid = builder.CategoryMock.CategoryLink.TargetId.Value;

            var parentId = Substitute.For<IIdentifier>();
            parentId.Value.Returns(categoryGuid);

            var parentLink = Substitute.For<ILink>();
            parentLink.TargetId.Returns(parentId);

            var condition = Substitute.For<IGameSystemCondition>();
            condition.ChildValueUnit.Returns(ConditionValueUnit.Percent);
            condition.ParentKind.Returns(ConditionParentKind.Reference);
            condition.ParentLink.Returns(parentLink);

            Assert.Equal(decimal.Divide(100m*pointsTotal, pointsLimit),
                CategoryChildValueExtractor.Extract(condition, builder));
        }

        [Fact]
        public void GameSystemCondition_CategoryPoints_Success()
        {
            const uint pointsTotal = 1250;
            var builder = CreateCategoryBuilder();

            var stats = Substitute.For<IStatAggregate>();
            stats.PointsTotal.Returns(pointsTotal);
            builder.StatAggregate.Returns(stats);

            var categoryGuid = builder.CategoryMock.CategoryLink.TargetId.Value;

            var parentId = Substitute.For<IIdentifier>();
            parentId.Value.Returns(categoryGuid);

            var parentLink = Substitute.For<ILink>();
            parentLink.TargetId.Returns(parentId);

            var condition = Substitute.For<IGameSystemCondition>();
            condition.ChildValueUnit.Returns(ConditionValueUnit.Points);
            condition.ParentKind.Returns(ConditionParentKind.Reference);
            condition.ParentLink.Returns(parentLink);

            Assert.Equal(pointsTotal, CategoryChildValueExtractor.Extract(condition, builder));
        }

        [Fact]
        public void GameSystemCondition_CategorySelections_Success()
        {
            const uint selectionCount = 23;
            var builder = CreateCategoryBuilder();

            var stats = Substitute.For<IStatAggregate>();
            stats.ChildSelectionsCount.Returns(selectionCount);
            builder.StatAggregate.Returns(stats);

            var categoryGuid = builder.CategoryMock.CategoryLink.TargetId.Value;

            var parentId = Substitute.For<IIdentifier>();
            parentId.Value.Returns(categoryGuid);

            var parentLink = Substitute.For<ILink>();
            parentLink.TargetId.Returns(parentId);

            var condition = Substitute.For<IGameSystemCondition>();
            condition.ChildValueUnit.Returns(ConditionValueUnit.Selections);
            condition.ParentKind.Returns(ConditionParentKind.Reference);
            condition.ParentLink.Returns(parentLink);

            Assert.Equal(selectionCount, CategoryChildValueExtractor.Extract(condition, builder));
        }

        [Fact]
        public void GameSystemCondition_NullArg1_Fail()
        {
            Assert.Throws<ArgumentNullException>(
                () => { CategoryChildValueExtractor.Extract(null, Substitute.For<IBuilderCore>()); });
        }

        [Fact]
        public void GameSystemCondition_NullArg2_Fail()
        {
            Assert.Throws<ArgumentNullException>(
                () => { CategoryChildValueExtractor.Extract(Substitute.For<IConditionCore>(), null); });
        }

        [Fact]
        public void GameSystemCondition_RosterPointsLimit_Success()
        {
            const uint pointsLimit = 1500;
            var builder = CreateCategoryBuilder();

            var roster = builder.AncestorContext.RosterBuilder.Roster;
            roster.PointsLimit.Returns(pointsLimit);

            var condition = Substitute.For<IGameSystemCondition>();
            condition.ParentKind.Returns(ConditionParentKind.Roster);
            condition.ChildValueUnit.Returns(ConditionValueUnit.PointsLimit);

            Assert.Equal(pointsLimit, CategoryChildValueExtractor.Extract(condition, builder));
        }

        [Fact]
        public void GameSystemCondition_RosterTotalSelections_Success()
        {
            const uint selectionCount = 23;
            var builder = CreateCategoryBuilder();

            var stats = Substitute.For<IStatAggregate>();
            stats.ChildSelectionsCount.Returns(selectionCount);

            var rosterBuilder = builder.AncestorContext.RosterBuilder;
            rosterBuilder.StatAggregate.Returns(stats);

            var condition = Substitute.For<IGameSystemCondition>();
            condition.ParentKind.Returns(ConditionParentKind.Roster);
            condition.ChildValueUnit.Returns(ConditionValueUnit.TotalSelections);

            Assert.Equal(selectionCount, CategoryChildValueExtractor.Extract(condition, builder));
        }

        private static ICategoryBuilder CreateCategoryBuilder()
        {
            var roster = Substitute.For<IRoster>();

            var rosterBuilder = Substitute.For<IRosterBuilder>();
            rosterBuilder.Roster.Returns(roster);

            var categoryGuid = Guid.NewGuid();

            var categoryId = Substitute.For<IIdentifier>();
            categoryId.Value.Returns(categoryGuid);

            var categoryLink = Substitute.For<IIdLink<ICategory>>();
            categoryLink.TargetId.Returns(categoryId);

            var categoryMock = Substitute.For<ICategoryMock>();
            categoryMock.CategoryLink.Returns(categoryLink);

            var categoryBuilder = Substitute.For<ICategoryBuilder>();
            categoryBuilder.CategoryMock.Returns(categoryMock);

            var forceBuilder = Substitute.For<IForceBuilder>();
            forceBuilder.CategoryBuilders.Returns(new[] {categoryBuilder});

            var ancestorContext = Substitute.For<IBuilderAncestorContext>();
            ancestorContext.ForceBuilder.Returns(forceBuilder);
            ancestorContext.RosterBuilder.Returns(rosterBuilder);

            categoryBuilder.AncestorContext.Returns(ancestorContext);

            return categoryBuilder;
        }

        private static INode<TItem, TFactoryArg> GetEmptyNodeSubstitute<TItem, TFactoryArg>()
            where TItem : INotifyPropertyChanged
        {
            var substitute = Substitute.For<INode<TItem, TFactoryArg>>();
            substitute.GetEnumerator().Returns(_ => Enumerable.Empty<TItem>().GetEnumerator());
            substitute.Count.Returns(0);
            return substitute;
        }

        private static INode<TItem, TFactoryArg> GetNodeSubstitute<TItem, TFactoryArg>(params TItem[] items)
            where TItem : INotifyPropertyChanged
        {
            return GetNodeSubstitute<TItem, TFactoryArg>(items.ToList());
        }

        private static INode<TItem, TFactoryArg> GetNodeSubstitute<TItem, TFactoryArg>(IEnumerable<TItem> items)
            where TItem : INotifyPropertyChanged
        {
            var list = items.ToList();
            var substitute = Substitute.For<INode<TItem, TFactoryArg>>();
            substitute.GetEnumerator().Returns(_ => list.GetEnumerator());
            substitute.Count.Returns(list.Count);
            return substitute;
        }
    }
}
