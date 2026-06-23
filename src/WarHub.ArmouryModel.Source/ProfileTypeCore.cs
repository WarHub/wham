using System.Collections.Immutable;
using System.Xml.Serialization;

namespace WarHub.ArmouryModel.Source
{
    [WhamNodeCore]
    [XmlType("profileType")]
    public sealed partial record ProfileTypeCore : CommentableCore
    {
        [XmlAttribute("id")]
        public string? Id { get; init; }

        [XmlAttribute("name")]
        public string? Name { get; init; }

        // NewRecruit addition. (Named KindValue to avoid colliding with SourceNode.Kind.)
        [XmlAttribute("kind")]
        public string? KindValue { get; init; }

        [XmlArray("characteristicTypes")]
        public ImmutableArray<CharacteristicTypeCore> CharacteristicTypes { get; init; } = ImmutableArray<CharacteristicTypeCore>.Empty;

        // NewRecruit addition.
        [XmlArray("attributeTypes")]
        public ImmutableArray<AttributeTypeCore> AttributeTypes { get; init; } = ImmutableArray<AttributeTypeCore>.Empty;
    }
}
