using System.Xml.Serialization;

namespace WarHub.ArmouryModel.Source
{
    [WhamNodeCore]
    [XmlType("characteristicType")]
    public sealed partial record CharacteristicTypeCore : CommentableCore
    {
        [XmlAttribute("id")]
        public string? Id { get; init; }

        [XmlAttribute("name")]
        public string? Name { get; init; }

        /// <inheritdoc />
        [XmlElement("comment")]
        public override string? Comment { get; init; }
    }
}
