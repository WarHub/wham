// WarHub licenses this file to you under the MIT license.
// See LICENSE file in the project root for more information.

namespace WarHub.Armoury.Model.DataAccess.Serialization
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Xml.Serialization;

    [XmlType("SystemIndex")]
    public class SerializableSystemIndex
    {
        [XmlArray("CatalogueInfos")]
        public List<SerializableCatalogueInfo> CatalogueInfosSerializable { get; set; }

        [XmlAttribute]
        public string GameSystemRawId { get; set; }

        [XmlArray("RosterInfos")]
        public List<SerializableRosterInfo> RosterInfosSerializable { get; set; }

        [XmlElement("GameSystemInfo")]
        public SerializableGameSystemInfo SerializableGameSystemInfo { get; set; }

        public static implicit operator SystemIndex(SerializableSystemIndex rhs)
        {
            if (rhs == null)
            {
                return default(SystemIndex);
            }
            return new SystemIndex(rhs);
        }

        public static implicit operator SerializableSystemIndex(SystemIndex rhs)
        {
            if (rhs == null)
            {
                return default(SerializableSystemIndex);
            }
            return new SerializableSystemIndex
            {
                CatalogueInfosSerializable = rhs.CatalogueInfos.Select(x => (SerializableCatalogueInfo) x).ToList(),
                SerializableGameSystemInfo = rhs.GameSystemInfo,
                GameSystemRawId = rhs.GameSystemRawId,
                RosterInfosSerializable = rhs.RosterInfos.Select(x => (SerializableRosterInfo) x).ToList()
            };
        }
    }
}
