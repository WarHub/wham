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

        [XmlArray("profiles")]
        public ImmutableArray<ProfileCore> Profiles { get; }

        [XmlArray("rules")]
        public ImmutableArray<RuleCore> Rules { get; }

        [XmlArray("infoGroups")]
        public ImmutableArray<InfoGroupCore> InfoGroups { get; }

        [XmlArray("infoLinks")]
        public ImmutableArray<InfoLinkCore> InfoLinks { get; }

        [XmlArray("forceEntries")]
        public ImmutableArray<ForceEntryCore> ForceEntries { get; }

        [XmlArray("categoryLinks")]
        public ImmutableArray<CategoryLinkCore> CategoryLinks { get; }
    }
}
