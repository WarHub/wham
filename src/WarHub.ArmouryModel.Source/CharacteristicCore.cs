using System.Xml.Serialization;

namespace WarHub.ArmouryModel.Source
{
    [WhamNodeCore]
    [XmlType("characteristic")]
    public partial class CharacteristicCore
    {
        [XmlAttribute("name")]
        public string Name { get; }

        [XmlAttribute("characteristicTypeId")]
        public string CharacteristicTypeId { get; }

        [XmlAttribute("value")]
        public string Value { get; }
    }
}
