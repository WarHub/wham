namespace WarHub.Armoury.Model.BattleScribeXml
{
    using System.Xml.Serialization;

    [XmlType("profileType")]
    public class ProfileType
    {
        [XmlArray("characteristicTypes", Order = 0)]
        public CharacteristicType[] CharacteristicTypes { get; set; }

        [XmlAttribute("id")]
        public string Id { get; set; }

        [XmlAttribute("name")]
        public string Name { get; set; }
    }
}