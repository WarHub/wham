// WarHub licenses this file to you under the MIT license.
// See LICENSE file in the project root for more information.

namespace WarHub.Armoury.Model.BattleScribeXml
{
    using System.Xml.Serialization;
    using Repo;

    [XmlType("dataIndexEntry")]
    public sealed class DataIndexEntry
    {
        [XmlAttribute("filePath")]
        public string FilePath { get; set; }

        [XmlAttribute("dataType")]
        public RemoteDataType DataType { get; set; }

        [XmlAttribute("dataId")]
        public string DataRawId { get; set; }

        [XmlAttribute("dataName")]
        public string DataName { get; set; }

        [XmlAttribute("dataBattleScribeVersion")]
        public string DataBattleScribeVersion { get; set; }

        [XmlAttribute("dataRevision")]
        public uint DataRevision { get; set; }

        public RemoteDataInfo CreateInfo()
        {
            return new RemoteDataInfo(FilePath, DataName, DataBattleScribeVersion, DataRawId, DataRevision, DataType);
        }

        public static DataIndexEntry CreateFromInfo(RemoteDataInfo info)
        {
            return new DataIndexEntry
            {
                FilePath = info.IndexPathSuffix,
                DataType = info.DataType,
                DataRawId = info.RawId,
                DataName = info.Name,
                DataBattleScribeVersion = info.OriginProgramVersion,
                DataRevision = info.Revision
            };
        }
    }
}
