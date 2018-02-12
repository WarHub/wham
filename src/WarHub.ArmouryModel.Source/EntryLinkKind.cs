using System.Xml.Serialization;

namespace WarHub.ArmouryModel.Source
{
    public enum EntryLinkKind
    {
        [XmlEnum("selectionEntry")]
        SelectionEntry,

        [XmlEnum("selectionEntryGroup")]
        SelectionEntryGroup
    }
}
