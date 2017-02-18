namespace WarHub.Armoury.Model.BattleScribeXml
{
    using System.Collections.Generic;
    using System.Xml.Serialization;

    [XmlType("selectionEntryGroup")]
    public class SelectionEntryGroup : EntryBase
    {
        [XmlArray("constraints", Order = 0)]
        public List<Constraint> Constraints { get; } = new List<Constraint>(0);

        [XmlArray("selectionEntries", Order = 1)]
        public List<SelectionEntry> SelectionEntries { get; } = new List<SelectionEntry>(0);

        [XmlArray("selectionEntryGroups", Order = 2)]
        public List<SelectionEntryGroup> SelectionEntryGroups { get; } = new List<SelectionEntryGroup>(0);

        [XmlArray("entryLinks", Order = 3)]
        public List<EntryLink> EntryLinks { get; } = new List<EntryLink>(0);

        [XmlAttribute("collective")]
        public bool Collective { get; set; }

        [XmlAttribute("defaultSelectionEntryId")]
        public string DefaultSelectionEntryId { get; set; }
    }
}