// WarHub licenses this file to you under the MIT license.
// See LICENSE file in the project root for more information.

namespace WarHub.Armoury.Model.Builders.Implementations
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using EntryTree;

    public class SelectionBuilder : BuilderCore, ISelectionBuilder
    {
        public SelectionBuilder(ISelection selection, IBuilderCore parentBuilder,
            IBuilderAncestorContext ancestorContext) : base(parentBuilder, ancestorContext)
        {
            if (selection == null)
                throw new ArgumentNullException(nameof(selection));
            if (parentBuilder == null)
                throw new ArgumentNullException(nameof(parentBuilder));
            if (ancestorContext == null)
                throw new ArgumentNullException(nameof(ancestorContext));
            Selection = selection;
            var entry = selection.OriginEntryPath.Target;
            EntryTreeRoot = TreeRoot.Create(entry);

            var childrenContext = AncestorContext.AppendedWith(this);
            var selectionMapping = EntryTreeRoot.MapSelections(selection);

            EntryBuilders =
                EntryTreeRoot.EntryNodes.Select(
                    node => new EntryBuilder(node, selectionMapping[node], this, childrenContext))
                    .ToArray();
            GroupBuilders =
                EntryTreeRoot.GroupNodes.Select(
                    node => new GroupBuilder(node, selectionMapping, this, childrenContext)).ToArray();
            ProfileBuilders =
                entry.GetProfileLinkPairs().Select(pair => new ProfileBuilder(pair, this, childrenContext)).ToArray();
            RuleBuilders =
                entry.GetRuleLinkPairs().Select(pair => new RuleBuilder(pair, this, childrenContext)).ToArray();
            StatAggregate = new SelectionStatAggregate(this);
        }

        public override IEnumerable<IBuilderCore> Children
            => EntryBuilders.Cast<IBuilderCore>().Concat(GroupBuilders).Concat(ProfileBuilders).Concat(RuleBuilders);

        public IEnumerable<IEntryBuilder> EntryBuilders { get; }

        public override IStatAggregate StatAggregate { get; }

        public IEnumerable<IGroupBuilder> GroupBuilders { get; }

        public INode EntryTreeRoot { get; }

        public IEnumerable<IProfileBuilder> ProfileBuilders { get; }

        public IEnumerable<IRuleBuilder> RuleBuilders { get; }

        public ISelection Selection { get; }

        public override bool IsForEntityId(Guid idValue) => false;

        public override void ApplyModifiers()
        {
            foreach (var builderCore in Children)
            {
                builderCore.ApplyModifiers();
            }
        }

        private class SelectionStatAggregate : StatAggregateBase
        {
            public SelectionStatAggregate(SelectionBuilder builder)
                : base(builder.Children.Select(core => core.StatAggregate))
            {
                Builder = builder;
            }

            public override uint ChildSelectionsCount
                => ChildrenAggregates.Aggregate(0u, (sum, aggregate) => sum + aggregate.ChildSelectionsCount);

            public override decimal PointsTotal
                =>
                    Builder.Selection.GetTotalPoints() +
                    ChildrenAggregates.Aggregate(0m, (sum, aggregate) => sum + aggregate.PointsTotal);

            private SelectionBuilder Builder { get; }

            protected override decimal GetChildPointsValue(Guid nodeGuid)
            {
                return
                    EntryBuilders()
                        .FirstOrDefault(builder => builder.EntryLinkPair.AnyHasId(nodeGuid))?
                        .SelectionBuilders.Select(builder => builder.Selection.GetTotalPoints())
                        .Sum() ?? 0m;
            }

            protected override uint GetChildSelectionCount(Guid selectionGuid)
            {
                return
                    (uint?)
                        EntryBuilders()
                            .FirstOrDefault(builder => builder.EntryLinkPair.AnyHasId(selectionGuid))?
                            .SelectionBuilders.Select(builder => (int) builder.Selection.NumberTaken)
                            .Sum() ?? 0u;
            }

            private IEnumerable<IEntryBuilder> EntryBuilders()
                => Builder.AllDescendants<IEntryBuilderNode, IEntryBuilderNode>(node => node.GroupBuilders)
                    .PrependWith(Builder)
                    .SelectMany(node => node.EntryBuilders);
        }
    }
}
