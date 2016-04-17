namespace WarHub.Armoury.Model.Builders
{
    using System.Collections.Generic;

    public interface IEntryBuilder : IApplicableEntryLimitsBuilder, IApplicableVisibilityBuilder
    {
        EntryLinkPair EntryLinkPair { get; }
        IEnumerable<ISelectionBuilder> SelectionBuilders { get; }
    }
}