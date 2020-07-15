using System.Collections.Immutable;
using System.Xml.Serialization;

namespace WarHub.ArmouryModel.Source
{
    [WhamNodeCore]
    public abstract partial class RosterElementBaseCore
    {
        [XmlAttribute("id")]
        public string? Id { get; }

        [XmlAttribute("name")]
        public string? Name { get; }

        [XmlAttribute("entryId")]
        public string? EntryId { get; }

        [XmlAttribute("entryGroupId")]
        public string? EntryGroupId { get; }

        [XmlAttribute("customName")]
        public string? CustomName { get; }

        [XmlElement("customNotes")]
        public string? CustomNotes { get; }

        [XmlAttribute("publicationId")]
        public string? PublicationId { get; }

        [XmlAttribute("page")]
        public string? Page { get; }

        [XmlArray("rules")]
        public ImmutableArray<RuleCore> Rules { get; }

        [XmlArray("profiles")]
        public ImmutableArray<ProfileCore> Profiles { get; }
    }
}
