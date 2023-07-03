using System.Collections.Immutable;
using System.Xml.Serialization;

namespace WarHub.ArmouryModel.Source
{
    [WhamNodeCore]
    [XmlType("profile")]
    public sealed partial record ProfileCore : EntryBaseCore
    {
        /// <inheritdoc />
        [XmlAttribute("id")]
        public override string? Id { get; init; }

        /// <inheritdoc />
        [XmlAttribute("name")]
        public override string? Name { get; init; }

        /// <inheritdoc />
        [XmlAttribute("publicationId")]
        public override string? PublicationId { get; init; }

        /// <inheritdoc />
        [XmlAttribute("page")]
        public override string? Page { get; init; }

        /// <inheritdoc />
        [XmlAttribute("hidden")]
        public override bool Hidden { get; init; }

        [XmlAttribute("typeId")]
        public string? TypeId { get; init; }

        [XmlAttribute("typeName")]
        public string? TypeName { get; init; }

        /// <inheritdoc />
        [XmlElement("comment")]
        public override string? Comment { get; init; }

        /// <inheritdoc />
        [XmlArray("modifiers")]
        public override ImmutableArray<ModifierCore> Modifiers { get; init; } = ImmutableArray<ModifierCore>.Empty;

        /// <inheritdoc />
        [XmlArray("modifierGroups")]
        public override ImmutableArray<ModifierGroupCore> ModifierGroups { get; init; } = ImmutableArray<ModifierGroupCore>.Empty;

        [XmlArray("characteristics")]
        public ImmutableArray<CharacteristicCore> Characteristics { get; init; } = ImmutableArray<CharacteristicCore>.Empty;
    }
}
