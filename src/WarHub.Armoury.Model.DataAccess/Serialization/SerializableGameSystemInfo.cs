// WarHub licenses this file to you under the MIT license.
// See LICENSE file in the project root for more information.

namespace WarHub.Armoury.Model.DataAccess.Serialization
{
    using System.Xml.Serialization;
    using Repo;

    [XmlType("GameSystemInfo")]
    public class SerializableGameSystemInfo
    {
        public string AuthorName { get; set; }

        public string Name { get; set; }

        public string OriginProgramVersion { get; set; }

        public string RawId { get; set; }

        public uint Revision { get; set; }

        public string Sourcebook { get; set; }

        public static implicit operator GameSystemInfo(SerializableGameSystemInfo rhs)
        {
            if (rhs == null)
            {
                return default(GameSystemInfo);
            }
            return new GameSystemInfo(
                rhs.Name,
                rhs.RawId,
                rhs.Revision,
                rhs.OriginProgramVersion,
                rhs.Sourcebook,
                rhs.AuthorName);
        }

        public static implicit operator SerializableGameSystemInfo(GameSystemInfo rhs)
        {
            if (rhs == null)
            {
                return default(SerializableGameSystemInfo);
            }
            return new SerializableGameSystemInfo
            {
                Name = rhs.Name,
                RawId = rhs.RawId,
                Revision = rhs.Revision,
                OriginProgramVersion = rhs.OriginProgramVersion,
                Sourcebook = rhs.Sourcebook,
                AuthorName = rhs.AuthorName
            };
        }
    }
}
