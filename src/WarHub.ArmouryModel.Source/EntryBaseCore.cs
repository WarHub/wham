using System.Collections.Immutable;
using System.Xml.Serialization;

namespace WarHub.ArmouryModel.Source
{
    [WhamNodeCore]
    public abstract partial class EntryBaseCore : CommentableCore
    {
        [XmlAttribute("id")]
        public string? Id { get; }

        [XmlAttribute("name")]
        public string? Name { get; }

        [XmlAttribute("publicationId")]
        public string? PublicationId { get; }

        [XmlAttribute("page")]
        public string? Page { get; }

        [XmlAttribute("hidden")]
        public bool Hidden { get; }

        [XmlArray("modifiers")]
        public ImmutableArray<ModifierCore> Modifiers { get; }

        [XmlArray("modifierGroups")]
        public ImmutableArray<ModifierGroupCore> ModifierGroups { get; }
    }
}
