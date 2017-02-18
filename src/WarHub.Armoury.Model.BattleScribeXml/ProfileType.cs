namespace WarHub.Armoury.Model.BattleScribeXml
{
    using System.Collections.Generic;
    using System.Xml.Serialization;

    [XmlType("profileType")]
    public class ProfileType
    {
        [XmlArray("characteristicTypes", Order = 0)]
        public List<CharacteristicType> CharacteristicTypes { get; } = new List<CharacteristicType>(0);

        [XmlAttribute("id")]
        public string Id { get; set; }

        [XmlAttribute("name")]
        public string Name { get; set; }
    }
}