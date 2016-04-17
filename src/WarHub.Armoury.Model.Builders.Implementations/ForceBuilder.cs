namespace WarHub.Armoury.Model.Builders.Implementations
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class ForceBuilder : BuilderCore, IForceBuilder
    {
        public ForceBuilder(IForce force, IBuilderCore parentBuilder, IBuilderAncestorContext ancestorContext)
            : base(parentBuilder, ancestorContext)
        {
            if (force == null) throw new ArgumentNullException(nameof(force));
            if (parentBuilder == null) throw new ArgumentNullException(nameof(parentBuilder));
            if (ancestorContext == null) throw new ArgumentNullException(nameof(ancestorContext));
            var childrenContext = AncestorContext.AppendedWith(this);
            InnerApplicableGeneralLimits.CopyFrom(force.ForceTypeLink.Target.Limits);
            Force = force;
            ForceBuilders = Force.Forces.Select(f => new ForceBuilder(f, this, childrenContext)).ToArray();
            CategoryBuilders =
                Force.CategoryMocks.Select(category => new CategoryBuilder(category, this, childrenContext)).ToArray();
            StatAggregate = new ForceStatAggregate(this);
        }

        private GeneralLimits InnerApplicableGeneralLimits { get; } = new GeneralLimits();
        public ILimits<int, decimal, int> ApplicableGeneralLimits => InnerApplicableGeneralLimits;

        public override IStatAggregate StatAggregate { get; }


        public override bool IsForEntityId(Guid idValue) => Force.ForceTypeLink.TargetIdValueEquals(idValue);

        public override void ApplyModifiers()
        {
            foreach (var builder in ForceBuilders)
            {
                builder.ApplyModifiers();
            }
            foreach (var builder in CategoryBuilders)
            {
                builder.ApplyModifiers();
            }
        }

        public override IEnumerable<IBuilderCore> Children
            => CategoryBuilders.Concat(ForceBuilders.Cast<IBuilderCore>());

        public IEnumerable<ICategoryBuilder> CategoryBuilders { get; }
        public IForce Force { get; }
        public IEnumerable<IForceBuilder> ForceBuilders { get; }

        private class ForceStatAggregate : StatAggregateBase
        {
            public ForceStatAggregate(IBuilderCore builder) : base(builder.Children.Select(core => core.StatAggregate))
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
