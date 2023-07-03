using System.Collections.Immutable;
using System.Xml.Serialization;

namespace WarHub.ArmouryModel.Source
{
    [WhamNodeCore]
    public abstract partial record ContainerEntryBaseCore : EntryBaseCore
    {
        [XmlArray("constraints")]
        public abstract ImmutableArray<ConstraintCore> Constraints { get; init; }

        [XmlArray("profiles")]
        public abstract ImmutableArray<ProfileCore> Profiles { get; init; }

        [XmlArray("rules")]
        public abstract ImmutableArray<RuleCore> Rules { get; init; }

        [XmlArray("infoGroups")]
        public abstract ImmutableArray<InfoGroupCore> InfoGroups { get; init; }

        [XmlArray("infoLinks")]
        public abstract ImmutableArray<InfoLinkCore> InfoLinks { get; init; }
    }
}
