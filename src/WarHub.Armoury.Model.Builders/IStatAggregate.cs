namespace WarHub.Armoury.Model.Builders
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    ///     Defines aggregated statistics, which can be easily aggregated again.
    /// </summary>
    public interface IStatAggregate
    {
        IEnumerable<IStatAggregate> ChildrenAggregates { get; }
        uint ChildSelectionsCount { get; }
        decimal PointsTotal { get; }
        decimal GetPointsTotal(Guid nodeGuid);
        uint GetSelectionCount(Guid selectionGuid);
    }
}
