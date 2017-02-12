namespace WarHub.Armoury.Model.BattleScribeXml
{
    using System.Xml.Serialization;

    [XmlType("entryLink")]
    public class EntryLink : EntryBase
    {
        [XmlArray("constraints", Order = 0)]
        public Constraint[] Constraints { get; set; }

        [XmlAttribute("targetId")]
        public string TargetId { get; set; }

        [XmlAttribute("type")]
        public EntryLinkKind Type { get; set; }

        [XmlAttribute("categoryEntryId")]
        public string CategoryEntryId { get; set; }
    }
}