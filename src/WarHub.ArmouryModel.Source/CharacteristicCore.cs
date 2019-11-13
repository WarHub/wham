using System.Xml.Serialization;

namespace WarHub.ArmouryModel.Source
{
    [WhamNodeCore]
    [XmlType("characteristic")]
    public sealed partial class CharacteristicCore
    {
        [XmlAttribute("name")]
        public string Name { get; }

        [XmlAttribute("typeId")]
        public string TypeId { get; }

        [XmlText]
        public string Value { get; }
    }
}
