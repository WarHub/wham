using System.Xml.Serialization;

namespace WarHub.ArmouryModel.Source
{
    [WhamNodeCore]
    [XmlType("dataIndexEntry")]
    public partial class DataIndexEntryCore
    {
        [XmlAttribute("filePath")]
        public string FilePath { get; set; }

        [XmlAttribute("dataType")]
        public DataIndexEntryKind DataType { get; set; }

        [XmlAttribute("dataId")]
        public string DataId { get; set; }

        [XmlAttribute("dataName")]
        public string DataName { get; set; }

        [XmlAttribute("dataBattleScribeVersion")]
        public string DataBattleScribeVersion { get; set; }

        [XmlAttribute("dataRevision")]
        public int DataRevision { get; set; }
    }
}
