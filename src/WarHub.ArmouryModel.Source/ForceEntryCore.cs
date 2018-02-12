using System.Collections.Immutable;
using System.Xml.Serialization;

namespace WarHub.ArmouryModel.Source
{
    [WhamNodeCore]
    [XmlType("forceEntry")]
    public partial class ForceEntryCore : EntryBaseCore
    {
        [XmlArray("constraints")]
        public ImmutableArray<ConstraintCore> Constraints { get; }

        [XmlArray("forceEntries")]
        public ImmutableArray<ForceEntryCore> ForceEntries { get; }

        [XmlArray("categoryLinks")]
        public ImmutableArray<CategoryLinkCore> CategoryLinks { get; }
    }
}
