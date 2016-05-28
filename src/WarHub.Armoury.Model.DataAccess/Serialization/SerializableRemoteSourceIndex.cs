// WarHub licenses this file to you under the MIT license.
// See LICENSE file in the project root for more information.

namespace WarHub.Armoury.Model.DataAccess.Serialization
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Xml.Serialization;
    using Repo;

    [XmlType("RemoteSourceIndex")]
    public class SerializableRemoteSourceIndex
    {
        [XmlArray("RemoteSources")]
        public List<SerializableRemoteSource> SerializableRemoteSources { get; set; }

        public static implicit operator RemoteSourceIndex(SerializableRemoteSourceIndex rhs)
        {
            if (rhs == null)
            {
                return default(RemoteSourceIndex);
            }
            return new RemoteSourceIndex(rhs.SerializableRemoteSources.Select(x => (RemoteSource) x));
        }

        public static implicit operator SerializableRemoteSourceIndex(RemoteSourceIndex rhs)
        {
            if (rhs == null)
            {
                return default(SerializableRemoteSourceIndex);
            }
            return new SerializableRemoteSourceIndex
            {
                SerializableRemoteSources =
                    rhs.RemoteSources.Select(x => (SerializableRemoteSource) x).ToList()
            };
        }
    }
}
