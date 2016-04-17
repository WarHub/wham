namespace WarHub.Armoury.Model.Builders
{
    using System.Collections.Generic;

    public interface ICategoryBuilder : IApplicableGeneralLimitsBuilder
    {
        ICategoryMock CategoryMock { get; }
        IEnumerable<IEntryBuilder> EntryBuilders { get; }
    }
}