namespace WarHub.Armoury.Model.BattleScribeXml
{
    using System.Xml.Serialization;

    [XmlType("characteristic")]
    public class Characteristic
    {
        [XmlAttribute("name")]
        public string Name { get; set; }

        [XmlAttribute("characteristicTypeId")]
        public string CharacteristicTypeId { get; set; }

        [XmlAttribute("value")]
        public string Value { get; set; }
    }
}