namespace WarHub.Armoury.Model.BattleScribeXml
{
    using System.Xml.Serialization;

    [XmlType("selectionEntryGroup")]
    public class SelectionEntryGroup : EntryBase
    {
        [XmlArray("constraints", Order = 0)]
        public Constraint[] Constraints { get; set; }

        [XmlArray("selectionEntries", Order = 1)]
        public SelectionEntry[] SelectionEntries { get; set; }

        [XmlArray("selectionEntryGroups", Order = 2)]
        public SelectionEntryGroup[] SelectionEntryGroups { get; set; }

        [XmlArray("entryLinks", Order = 3)]
        public EntryLink[] EntryLinks { get; set; }

        [XmlAttribute("collective")]
        public bool Collective { get; set; }

        [XmlAttribute("defaultSelectionEntryId")]
        public string DefaultSelectionEntryId { get; set; }
    }
}