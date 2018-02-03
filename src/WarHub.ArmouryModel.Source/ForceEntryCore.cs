using System.Collections.Immutable;
using System.Xml.Serialization;

namespace WarHub.ArmouryModel.Source
{
    [WhamNodeCore]
    [XmlType("forceEntry")]
    public partial class ForceEntryCore : EntryBaseCore
    {
        [XmlArray("constraints", Order = 0)]
        public ImmutableArray<ConstraintCore> Constraints { get; }

        [XmlArray("categoryEntries", Order = 1)]
        public ImmutableArray<CategoryEntryCore> CategoryEntries { get; }

        [XmlArray("forceEntries", Order = 2)]
        public ImmutableArray<ForceEntryCore> ForceEntries { get; }
    }
}
