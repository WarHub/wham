namespace WarHub.Armoury.Model.Builders
{
    using System.Collections.Generic;

    public interface IEntryBuilderNode
    {
        IEnumerable<IEntryBuilder> EntryBuilders { get; }
        IEnumerable<IGroupBuilder> GroupBuilders { get; }
    }
}