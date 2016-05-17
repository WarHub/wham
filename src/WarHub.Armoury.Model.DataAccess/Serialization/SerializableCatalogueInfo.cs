// WarHub licenses this file to you under the MIT license.
// See LICENSE file in the project root for more information.

namespace WarHub.Armoury.Model.DataAccess.Serialization
{
    using System.Xml.Serialization;
    using Repo;

    [XmlType("CatalogueInfo")]
    public class SerializableCatalogueInfo
    {
        public string AuthorName { get; set; }

        public string GameSystemRawId { get; set; }

        public string Name { get; set; }

        public string OriginProgramVersion { get; set; }

        public string RawId { get; set; }

        public uint Revision { get; set; }

        public string Sourcebook { get; set; }

        public static implicit operator CatalogueInfo(SerializableCatalogueInfo rhs)
        {
            if (rhs == null)
            {
                return default(CatalogueInfo);
            }
            return new CatalogueInfo(
                rhs.Name,
                rhs.RawId,
                rhs.Revision,
                rhs.GameSystemRawId,
                rhs.OriginProgramVersion,
                rhs.Sourcebook,
                rhs.AuthorName);
        }

        public static implicit operator SerializableCatalogueInfo(CatalogueInfo rhs)
        {
            if (rhs == null)
            {
                return default(SerializableCatalogueInfo);
            }
            return new SerializableCatalogueInfo
            {
                Name = rhs.Name,
                RawId = rhs.RawId,
                Revision = rhs.Revision,
                GameSystemRawId = rhs.GameSystemRawId,
                OriginProgramVersion = rhs.OriginProgramVersion,
                Sourcebook = rhs.Sourcebook,
                AuthorName = rhs.AuthorName
            };
        }
    }
}
