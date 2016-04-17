// WarHub licenses this file to you under the MIT license.
// See LICENSE file in the project root for more information.

namespace WarHub.Armoury.Model.Builders.Implementations
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class RosterBuilder : BuilderCore, IRosterBuilder
    {
        public RosterBuilder(IRoster roster) : base(null, null)
        {
            if (roster == null)
                throw new ArgumentNullException(nameof(roster));
            Roster = roster;
            var childrenContext = BuilderAncestorContext.Create(this);
            ForceBuilders = Roster.Forces.Select(force => new ForceBuilder(force, this, childrenContext)).ToArray();
            StatAggregate = new RosterStatAggregate(this);
        }

        public override IStatAggregate StatAggregate { get; }

        public override bool IsForEntityId(Guid idValue) => false;

        public override void ApplyModifiers()
        {
            foreach (var builderCore in Children)
            {
                builderCore.ApplyModifiers();
            }
        }

        public override IEnumerable<IBuilderCore> Children => ForceBuilders;

        public IEnumerable<IForceBuilder> ForceBuilders { get; }

        public IRoster Roster { get; }

        private class RosterStatAggregate : StatAggregateBase
        {
            public RosterStatAggregate(IBuilderCore builder) : base(builder.Children.Select(core => core.StatAggregate))
            {
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
