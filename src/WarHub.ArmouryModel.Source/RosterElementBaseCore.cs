using System.Collections.Immutable;
using System.Xml.Serialization;

namespace WarHub.ArmouryModel.Source
{
    [WhamNodeCore]
    public abstract partial record RosterElementBaseCore
    {
        [XmlAttribute("id")]
        public string? Id { get; init; }

        [XmlAttribute("name")]
        public string? Name { get; init; }

        [XmlAttribute("entryId")]
        public string? EntryId { get; init; }

        [XmlAttribute("entryGroupId")]
        public string? EntryGroupId { get; init; }

        [XmlAttribute("customName")]
        public string? CustomName { get; init; }

        [XmlElement("customNotes")]
        public string? CustomNotes { get; init; }

        [XmlAttribute("publicationId")]
        public string? PublicationId { get; init; }

        [XmlAttribute("page")]
        public string? Page { get; init; }

        [XmlArray("rules")]
        public ImmutableArray<RuleCore> Rules { get; init; } = ImmutableArray<RuleCore>.Empty;

        [XmlArray("profiles")]
        public ImmutableArray<ProfileCore> Profiles { get; init; } = ImmutableArray<ProfileCore>.Empty;
    }
}
