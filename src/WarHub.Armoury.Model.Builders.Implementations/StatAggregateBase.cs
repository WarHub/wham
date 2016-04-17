// WarHub licenses this file to you under the MIT license.
// See LICENSE file in the project root for more information.

namespace WarHub.Armoury.Model.Builders.Implementations
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public abstract class StatAggregateBase : IStatAggregate
    {
        protected StatAggregateBase(IEnumerable<IStatAggregate> childrenAggregates)
        {
            ChildrenAggregates = childrenAggregates;
        }

        public IEnumerable<IStatAggregate> ChildrenAggregates { get; }

        public abstract uint ChildSelectionsCount { get; }

        public decimal GetPointsTotal(Guid nodeGuid)
        {
            return GetChildPointsValue(nodeGuid) +
                   ChildrenAggregates.Select(aggregate => aggregate.GetPointsTotal(nodeGuid)).Sum();
        }

        public uint GetSelectionCount(Guid selectionGuid)
        {
            return GetChildSelectionCount(selectionGuid) +
                   ChildrenAggregates.Aggregate(0u, (sum, aggregate) => sum + aggregate.GetSelectionCount(selectionGuid));
        }

        public abstract decimal PointsTotal { get; }

        protected abstract decimal GetChildPointsValue(Guid nodeGuid);
        protected abstract uint GetChildSelectionCount(Guid selectionGuid);
    }
}
