namespace WarHub.Armoury.Model.BattleScribeXml
{
    using System.Xml.Serialization;

    [XmlType("infoLink")]
    public class InfoLink : EntryBase
    {
        [XmlAttribute("targetId")]
        public string TargetId { get; set; }

        [XmlAttribute("type")]
        public InfoLinkKind Type { get; set; }

        [XmlIgnore]
        public bool TypeSpecified { get; set; }
    }
}