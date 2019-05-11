using System.Xml.Serialization;

namespace WarHub.ArmouryModel.Source
{
    [WhamNodeCore]
    [XmlType("characteristic")]
    public partial class CharacteristicCore
    {
        [XmlAttribute("name")]
        public string Name { get; }

        [XmlAttribute("typeId")]
        public string CharacteristicTypeId { get; }

        [XmlText]
        public string Value { get; }
    }
}
