// WarHub licenses this file to you under the MIT license.
// See LICENSE file in the project root for more information.

namespace WarHub.Armoury.Model.DataAccess.Serialization
{
    using System.Xml.Serialization;
    using Repo;

    [XmlType("RemoteSource")]
    public class SerializableRemoteSource
    {
        public string IndexUri { get; set; }

        public string Name { get; set; }

        public override string ToString()
        {
            return $"[{Name}]({IndexUri})";
        }

        public static implicit operator RemoteSource(SerializableRemoteSource rhs)
        {
            if (rhs == null)
            {
                return default(RemoteSource);
            }
            return new RemoteSource(rhs.Name, rhs.IndexUri);
        }

        public static implicit operator SerializableRemoteSource(RemoteSource rhs)
        {
            if (rhs == null)
            {
                return default(SerializableRemoteSource);
            }
            return new SerializableRemoteSource
            {
                Name = rhs.Name,
                IndexUri = rhs.IndexUri
            };
        }
    }
}
