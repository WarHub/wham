using System.Collections.Immutable;
using System.Xml.Serialization;

namespace WarHub.ArmouryModel.Source
{
    [WhamNodeCore]
    public abstract partial class RosterElementBaseCore
    {
        [XmlAttribute("id")]
        public string Id { get; }

        [XmlAttribute("name")]
        public string Name { get; }

        [XmlAttribute("entryId")]
        public string EntryId { get; }

        [XmlArray("rules", Order = 0)]
        public ImmutableArray<RuleCore> Rules { get; }

        [XmlArray("profiles", Order = 1)]
        public ImmutableArray<ProfileCore> Profiles { get; }
    }
}
