// WarHub licenses this file to you under the MIT license.
// See LICENSE file in the project root for more information.

namespace WarHub.Armoury.Model.DataAccess.Serialization
{
    using System.Xml.Serialization;
    using Repo;

    [XmlType("RosterInfo")]
    public class SerializableRosterInfo
    {
        public string GameSystemRawId { get; set; }

        public string Name { get; set; }

        public string OriginProgramVersion { get; set; }

        public decimal PointsLimit { get; set; }

        public decimal PointsValue { get; set; }

        public string RawId { get; set; }

        public static implicit operator RosterInfo(SerializableRosterInfo rhs)
        {
            if (rhs == null)
            {
                return default(RosterInfo);
            }
            return new RosterInfo(
                rhs.Name,
                rhs.RawId,
                rhs.GameSystemRawId,
                rhs.OriginProgramVersion,
                rhs.PointsValue,
                rhs.PointsLimit);
        }

        public static implicit operator SerializableRosterInfo(RosterInfo rhs)
        {
            if (rhs == null)
            {
                return default(SerializableRosterInfo);
            }
            return new SerializableRosterInfo
            {
                Name = rhs.Name,
                RawId = rhs.RawId,
                GameSystemRawId = rhs.GameSystemRawId,
                OriginProgramVersion = rhs.OriginProgramVersion,
                PointsValue = rhs.PointsValue,
                PointsLimit = rhs.PointsLimit
            };
        }
    }
}
