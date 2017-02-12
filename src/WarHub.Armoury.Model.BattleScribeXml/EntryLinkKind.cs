namespace WarHub.Armoury.Model.BattleScribeXml
{
    using System.Xml.Serialization;

    public enum EntryLinkKind
    {
        [XmlEnum("selectionEntry")] SelectionEntry,
        [XmlEnum("selectionEntryGroup")] SelectionEntryGroup
    }
}