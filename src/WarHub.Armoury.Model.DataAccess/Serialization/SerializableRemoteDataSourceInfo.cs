// WarHub licenses this file to you under the MIT license.
// See LICENSE file in the project root for more information.

namespace WarHub.Armoury.Model.DataAccess.Serialization
{
    using System.Xml.Serialization;

    [XmlType("RemoteDataSourceInfo")]
    public class SerializableRemoteDataSourceInfo
    {
        public string IndexUri { get; set; }

        public string Name { get; set; }

        public override string ToString()
        {
            return $"[{Name}]({IndexUri})";
        }

        public static implicit operator RemoteDataSourceInfo(SerializableRemoteDataSourceInfo rhs)
        {
            if (rhs == null)
            {
                return default(RemoteDataSourceInfo);
            }
            return new RemoteDataSourceInfo(rhs.Name, rhs.IndexUri);
        }

        public static implicit operator SerializableRemoteDataSourceInfo(RemoteDataSourceInfo rhs)
        {
            if (rhs == null)
            {
                return default(SerializableRemoteDataSourceInfo);
            }
            return new SerializableRemoteDataSourceInfo
            {
                Name = rhs.Name,
                IndexUri = rhs.IndexUri
            };
        }
    }
}
