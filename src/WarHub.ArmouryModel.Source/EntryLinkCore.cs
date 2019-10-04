using System.Collections.Immutable;
using System.Xml.Serialization;

namespace WarHub.ArmouryModel.Source
{
    [WhamNodeCore]
    [XmlType("entryLink")]
    public partial class EntryLinkCore : EntryBaseCore
    {
        [XmlAttribute("collective")]
        public bool Collective { get; }

        [XmlAttribute("import")]
        public bool Import { get; }

        [XmlAttribute("targetId")]
        public string TargetId { get; }

        [XmlAttribute("type")]
        public EntryLinkKind Type { get; }

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

        [XmlArray("categoryLinks")]
        public ImmutableArray<CategoryLinkCore> CategoryLinks { get; }

        [XmlArray("costs")]
        public ImmutableArray<CostCore> Costs { get; }
    }
}
