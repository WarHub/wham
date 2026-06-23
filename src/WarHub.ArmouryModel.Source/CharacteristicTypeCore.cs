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

        // NewRecruit additions. (KindValue avoids colliding with SourceNode.Kind.)
        [XmlAttribute("kind")]
        public string? KindValue { get; init; }

        [XmlAttribute("defaultValue")]
        public string? DefaultValue { get; init; }
    }
}
