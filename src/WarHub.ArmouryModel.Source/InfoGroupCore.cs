using System.Collections.Immutable;
using System.Xml.Serialization;

namespace WarHub.ArmouryModel.Source
{
    [WhamNodeCore]
    [XmlType("infoGroup")]
    public sealed partial record InfoGroupCore : EntryBaseCore
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

        /// <inheritdoc />
        [XmlElement("comment")]
        public override string? Comment { get; init; }

        /// <inheritdoc />
        [XmlArray("modifiers")]
        public override ImmutableArray<ModifierCore> Modifiers { get; init; } = ImmutableArray<ModifierCore>.Empty;

        /// <inheritdoc />
        [XmlArray("modifierGroups")]
        public override ImmutableArray<ModifierGroupCore> ModifierGroups { get; init; } = ImmutableArray<ModifierGroupCore>.Empty;

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
