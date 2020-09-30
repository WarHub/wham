using System.Xml.Serialization;

namespace WarHub.ArmouryModel.Source
{
    [WhamNodeCore]
    [XmlType("selectionEntryGroup")]
    public sealed partial record SelectionEntryGroupCore : SelectionEntryBaseCore
    {
        [XmlAttribute("defaultSelectionEntryId")]
        public string? DefaultSelectionEntryId { get; init; }
    }
}
