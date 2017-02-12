namespace WarHub.Armoury.Model.BattleScribeXml
{
    using System.Xml.Serialization;

    [XmlType("selectionEntry")]
    public class SelectionEntry : EntryBase
    {
        [XmlArray("constraints", Order = 0)]
        public Constraint[] Constraints { get; set; }

        [XmlArray("selectionEntries", Order = 1)]
        public SelectionEntry[] SelectionEntries { get; set; }

        [XmlArray("selectionEntryGroups", Order = 2)]
        public SelectionEntryGroup[] SelectionEntryGroups { get; set; }

        [XmlArray("entryLinks", Order = 3)]
        public EntryLink[] EntryLinks { get; set; }

        [XmlArray("costs", Order = 4)]
        public Cost[] Costs { get; set; }

        [XmlAttribute("collective")]
        public bool Collective { get; set; }

        [XmlAttribute("categoryEntryId")]
        public string CategoryEntryId { get; set; }

        [XmlAttribute("type")]
        public SelectionEntryKind Type { get; set; }
    }
}