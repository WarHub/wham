namespace WarHub.Armoury.Model.Builders.Implementations
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using EntryTree;

    public class EntryBuilder : BuilderCore, IEntryBuilder
    {
        public EntryBuilder(IEntryNode entryNode, IEnumerable<ISelection> selections, IBuilderCore parentBuilder,
            IBuilderAncestorContext ancestorContext) : base(parentBuilder, ancestorContext)
        {
            if (entryNode == null) throw new ArgumentNullException(nameof(entryNode));
            if (parentBuilder == null) throw new ArgumentNullException(nameof(parentBuilder));
            if (ancestorContext == null) throw new ArgumentNullException(nameof(ancestorContext));
            var childrenContext = AncestorContext.AppendedWith(this);
            EntryNode = entryNode;
            InnerApplicableEntryLimits.CopyFrom(entryNode.Entry);
            InnerApplicableVisibility.CopyFrom(entryNode.Entry);
            SelectionBuilders =
                selections.Select(selection => new SelectionBuilder(selection, this, childrenContext)).ToArray();
            StatAggregate = new EntryStatAggregate(this);
        }

        public IEntryNode EntryNode { get; }
        private EntryLimits InnerApplicableEntryLimits { get; } = new EntryLimits();
        private ApplicableVisibility InnerApplicableVisibility { get; } = new ApplicableVisibility();
        public IEntryLimits ApplicableEntryLimits => InnerApplicableEntryLimits;
        public IApplicableVisibility ApplicableVisibility => InnerApplicableVisibility;

        public override IStatAggregate StatAggregate { get; }


        public override bool IsForEntityId(Guid idValue) => EntryLinkPair.AnyHasId(idValue);

        public override void ApplyModifiers()
        {
            foreach (var builderCore in Children)
            {
                builderCore.ApplyModifiers();
            }
        }

        public override IEnumerable<IBuilderCore> Children => SelectionBuilders;
        public EntryLinkPair EntryLinkPair => EntryNode.EntryLinkPair;
        public IEnumerable<ISelectionBuilder> SelectionBuilders { get; }

        private class EntryStatAggregate : StatAggregateBase
        {
            public EntryStatAggregate(EntryBuilder builder) : base(builder.Children.Select(core => core.StatAggregate))
            {
                Builder = builder;
            }

            private EntryBuilder Builder { get; }

            public override uint ChildSelectionsCount
                => Builder.SelectionBuilders.Aggregate(0u, (sum, builder) => builder.Selection.NumberTaken);

            public override decimal PointsTotal => ChildrenAggregates.Select(aggregate => aggregate.PointsTotal).Sum();

            protected override decimal GetChildPointsValue(Guid nodeGuid) => 0m;

            protected override uint GetChildSelectionCount(Guid selectionGuid) => 0u;
        }
    }
}
