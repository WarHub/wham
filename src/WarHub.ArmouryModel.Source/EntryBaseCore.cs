using System.Collections.Immutable;
using System.Xml.Serialization;

namespace WarHub.ArmouryModel.Source
{
    [WhamNodeCore]
    public abstract partial class EntryBaseCore
    {
        [XmlAttribute("id")]
        public string Id { get; }

        [XmlAttribute("name")]
        public string Name { get; }

        [XmlAttribute("book")]
        public string Book { get; }

        [XmlAttribute("page")]
        public string Page { get; }

        [XmlAttribute("hidden")]
        public bool IsHidden { get; }

        [XmlArray("profiles")]
        public ImmutableArray<ProfileCore> Profiles { get; }

        [XmlArray("rules")]
        public ImmutableArray<RuleCore> Rules { get; }

        [XmlArray("infoLinks")]
        public ImmutableArray<InfoLinkCore> InfoLinks { get; }

        [XmlArray("modifiers")]
        public ImmutableArray<ModifierCore> Modifiers { get; }
    }
}
