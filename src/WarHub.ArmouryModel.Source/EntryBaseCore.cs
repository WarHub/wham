using System.Collections.Immutable;
using System.Xml.Serialization;

namespace WarHub.ArmouryModel.Source
{
    [WhamNodeCore]
    public abstract partial record EntryBaseCore : CommentableCore
    {
        [XmlAttribute("id")]
        public abstract string? Id { get; init; }

        [XmlAttribute("name")]
        public abstract string? Name { get; init; }

        [XmlAttribute("publicationId")]
        public abstract string? PublicationId { get; init; }

        [XmlAttribute("page")]
        public abstract string? Page { get; init; }

        [XmlAttribute("hidden")]
        public abstract bool Hidden { get; init; }

        [XmlArray("modifiers")]
        public abstract ImmutableArray<ModifierCore> Modifiers { get; init; }

        [XmlArray("modifierGroups")]
        public abstract ImmutableArray<ModifierGroupCore> ModifierGroups { get; init; }
    }
}
