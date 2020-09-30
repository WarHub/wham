using System.Xml.Serialization;

namespace WarHub.ArmouryModel.Source
{
    [WhamNodeCore]
    [XmlType("characteristic")]
    public sealed partial record CharacteristicCore
    {
        [XmlAttribute("name")]
        public string? Name { get; init; }

        [XmlAttribute("typeId")]
        public string? TypeId { get; init; }

        [XmlText]
        public string? Value { get; init; }
    }
}
