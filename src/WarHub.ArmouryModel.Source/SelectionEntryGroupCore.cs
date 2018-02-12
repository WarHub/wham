using System.Xml.Serialization;

namespace WarHub.ArmouryModel.Source
{
    [WhamNodeCore]
    [XmlType("selectionEntryGroup")]
    public partial class SelectionEntryGroupCore : SelectionEntryBaseCore
    {
        [XmlAttribute("defaultSelectionEntryId")]
        public string DefaultSelectionEntryId { get; }
    }
}
