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

        /// <inheritdoc />
        [XmlElement("comment")]
        public override string? Comment { get; init; }

        [XmlArray("characteristicTypes")]
        public ImmutableArray<CharacteristicTypeCore> CharacteristicTypes { get; init; } = ImmutableArray<CharacteristicTypeCore>.Empty;
    }
}
