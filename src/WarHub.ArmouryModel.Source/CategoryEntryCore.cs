using System.Collections.Immutable;
using System.Xml.Serialization;

namespace WarHub.ArmouryModel.Source
{
    [WhamNodeCore]
    [XmlType("categoryEntry")]
    public partial class CategoryEntryCore : EntryBaseCore
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
    }
}
