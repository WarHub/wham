using System.Collections.Immutable;
using System.Xml.Serialization;

namespace WarHub.ArmouryModel.Source
{
    [WhamNodeCore]
    public abstract partial record EntryBaseCore : CommentableCore
    {
        [XmlAttribute("id")]
        public string? Id { get; init; }

        [XmlAttribute("name")]
        public string? Name { get; init; }

        [XmlAttribute("publicationId")]
        public string? PublicationId { get; init; }

        [XmlAttribute("page")]
        public string? Page { get; init; }

        [XmlAttribute("hidden")]
        public bool Hidden { get; init; }

        [XmlArray("modifiers")]
        public ImmutableArray<ModifierCore> Modifiers { get; init; } = ImmutableArray<ModifierCore>.Empty;

        [XmlArray("modifierGroups")]
        public ImmutableArray<ModifierGroupCore> ModifierGroups { get; init; } = ImmutableArray<ModifierGroupCore>.Empty;
    }
}
