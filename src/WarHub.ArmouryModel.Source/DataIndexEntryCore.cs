using System.Xml.Serialization;

namespace WarHub.ArmouryModel.Source
{
    [WhamNodeCore]
    [XmlType("dataIndexEntry")]
    public sealed partial class DataIndexEntryCore
    {
        [XmlAttribute("filePath")]
        public string? FilePath { get; }

        [XmlAttribute("dataType")]
        public DataIndexEntryKind DataType { get; }

        [XmlAttribute("dataId")]
        public string? DataId { get; }

        [XmlAttribute("dataName")]
        public string? DataName { get; }

        [XmlAttribute("dataBattleScribeVersion")]
        public string? DataBattleScribeVersion { get; }

        [XmlAttribute("dataRevision")]
        public int DataRevision { get; }
    }
}
