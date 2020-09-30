using System.Xml.Serialization;

namespace WarHub.ArmouryModel.Source
{
    [WhamNodeCore]
    [XmlType("dataIndexEntry")]
    public sealed partial record DataIndexEntryCore
    {
        [XmlAttribute("filePath")]
        public string? FilePath { get; init; }

        [XmlAttribute("dataType")]
        public DataIndexEntryKind DataType { get; init; }

        [XmlAttribute("dataId")]
        public string? DataId { get; init; }

        [XmlAttribute("dataName")]
        public string? DataName { get; init; }

        [XmlAttribute("dataBattleScribeVersion")]
        public string? DataBattleScribeVersion { get; init; }

        [XmlAttribute("dataRevision")]
        public int DataRevision { get; init; }
    }
}
