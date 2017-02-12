namespace WarHub.Armoury.Model.BattleScribeXml
{
    using System.Xml.Serialization;

    [XmlType("profile")]
    public class Profile : EntryBase
    {
        [XmlArray("characteristics", Order = 0)]
        public Characteristic[] Characteristics { get; set; }

        [XmlAttribute("profileTypeId")]
        public string ProfileTypeId { get; set; }

        [XmlAttribute("profileTypeName")]
        public string ProfileTypeName { get; set; }
    }
}