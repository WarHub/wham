using System.Collections.Immutable;
using System.Xml.Serialization;

namespace WarHub.ArmouryModel.Source
{
    [WhamNodeCore]
    [XmlType("infoGroup")]
    public sealed partial record InfoGroupCore : EntryBaseCore
    {
        [XmlArray("profiles")]
        public ImmutableArray<ProfileCore> Profiles { get; init; } = ImmutableArray<ProfileCore>.Empty;

        [XmlArray("rules")]
        public ImmutableArray<RuleCore> Rules { get; init; } = ImmutableArray<RuleCore>.Empty;

        [XmlArray("infoGroups")]
        public ImmutableArray<InfoGroupCore> InfoGroups { get; init; } = ImmutableArray<InfoGroupCore>.Empty;

        [XmlArray("infoLinks")]
        public ImmutableArray<InfoLinkCore> InfoLinks { get; init; } = ImmutableArray<InfoLinkCore>.Empty;
    }
}
