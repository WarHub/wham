// WarHub licenses this file to you under the MIT license.
// See LICENSE file in the project root for more information.

namespace WarHub.Armoury.Model.DataAccess.Serialization
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Xml.Serialization;

    [XmlType("DataIndex")]
    public class SerializableDataIndex
    {
        [XmlArray("SystemIndexes")]
        public List<SerializableSystemIndex> SystemIndexesSerializable { get; set; }

        public static implicit operator DataIndex(SerializableDataIndex rhs)
        {
            if (rhs == null)
            {
                return default(DataIndex);
            }
            return new DataIndex(rhs.SystemIndexesSerializable.Select(x => (SystemIndex) x));
        }

        public static implicit operator SerializableDataIndex(DataIndex rhs)
        {
            if (rhs == null)
            {
                return default(SerializableDataIndex);
            }
            return new SerializableDataIndex
            {
                SystemIndexesSerializable =
                    rhs.SystemIndexes.Select(x => (SerializableSystemIndex) (x as SystemIndex)).ToList()
            };
        }
    }
}
