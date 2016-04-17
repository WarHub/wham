// WarHub licenses this file to you under the MIT license.
// See LICENSE file in the project root for more information.

namespace WarHub.Armoury.Model.Builders.Implementations
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using EntryTree;

    public class GroupBuilder : BuilderCore, IGroupBuilder
    {
        public GroupBuilder(IGroupNode groupNode, IReadOnlyDictionary<IEntryNode, List<ISelection>> selectionMapping,
            IBuilderCore parentBuilder, IBuilderAncestorContext ancestorContext) : base(parentBuilder, ancestorContext)
        {
            if (groupNode == null)
                throw new ArgumentNullException(nameof(groupNode));
            if (selectionMapping == null)
                throw new ArgumentNullException(nameof(selectionMapping));
            if (parentBuilder == null)
                throw new ArgumentNullException(nameof(parentBuilder));
            if (ancestorContext == null)
                throw new ArgumentNullException(nameof(ancestorContext));
            var childrenContext = AncestorContext.AppendedWith(this);
            GroupLinkPair = groupNode.GroupLinkPair;
            InnerApplicableEntryLimits.CopyFrom(groupNode.Group);
            InnerApplicableVisibility.CopyFrom(groupNode.Group);
            EntryBuilders =
                groupNode.EntryNodes.Select(
                    node => new EntryBuilder(node, selectionMapping[node], this, childrenContext))
                    .ToArray();
            GroupBuilders =
                groupNode.GroupNodes.Select(node => new GroupBuilder(node, selectionMapping, this, childrenContext))
                    .ToArray();
            StatAggregate = new GroupStatAggregate(this);
        }

        private EntryLimits InnerApplicableEntryLimits { get; } = new EntryLimits();

        private ApplicableVisibility InnerApplicableVisibility { get; } = new ApplicableVisibility();

        public IEntryLimits ApplicableEntryLimits => InnerApplicableEntryLimits;

        public IApplicableVisibility ApplicableVisibility => InnerApplicableVisibility;

        public override IStatAggregate StatAggregate { get; }

        public override bool IsForEntityId(Guid idValue) => GroupLinkPair.AnyHasId(idValue);

        public override void ApplyModifiers()
        {
            foreach (var builderCore in Children)
            {
                builderCore.ApplyModifiers();
            }
        }

        public override IEnumerable<IBuilderCore> Children => EntryBuilders.Concat(GroupBuilders.Cast<IBuilderCore>());

        public IEnumerable<IEntryBuilder> EntryBuilders { get; }

        public IEnumerable<IGroupBuilder> GroupBuilders { get; }

        public GroupLinkPair GroupLinkPair { get; }

        private class GroupStatAggregate : StatAggregateBase
        {
            public GroupStatAggregate(IBuilderCore builder) : base(builder.Children.Select(core => core.StatAggregate))
            {
                //TODO include or not "count towards parent" appropriately - IMPORTANT
            }

            public override uint ChildSelectionsCount
                => ChildrenAggregates.Aggregate(0u, (sum, aggregate) => sum + aggregate.ChildSelectionsCount);

            public override decimal PointsTotal
                => ChildrenAggregates.Aggregate(0m, (sum, aggregate) => sum + aggregate.PointsTotal);

            protected override decimal GetChildPointsValue(Guid nodeGuid) => 0m;

            protected override uint GetChildSelectionCount(Guid selectionGuid) => 0u;
        }
    }
}
