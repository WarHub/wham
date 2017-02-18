namespace WarHub.Armoury.Model.BattleScribeXml
{
    using System.Collections.Generic;
    using System.Xml.Serialization;

    [XmlType("profile")]
    public class Profile : EntryBase
    {
        [XmlArray("characteristics", Order = 0)]
        public List<Characteristic> Characteristics { get; } = new List<Characteristic>(0);

        [XmlAttribute("profileTypeId")]
        public string ProfileTypeId { get; set; }

        [XmlAttribute("profileTypeName")]
        public string ProfileTypeName { get; set; }
    }
}