// WarHub licenses this file to you under the MIT license.
// See LICENSE file in the project root for more information.

namespace WarHub.Armoury.Model.Repo
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Xml;

    public sealed class RosterInfo
    {
        public RosterInfo(
            string name,
            string rawId,
            string gameSystemRawId,
            string originProgramVersion,
            decimal pointsValue,
            decimal pointsLimit)
        {
            Name = name;
            RawId = rawId;
            GameSystemRawId = gameSystemRawId;
            OriginProgramVersion = originProgramVersion;
            PointsValue = pointsValue;
            PointsLimit = pointsLimit;
        }

        public RosterInfo(IRoster roster)
        {
            if (roster == null)
                throw new ArgumentNullException(nameof(roster));
            Name = roster.Name;
            RawId = roster.Id.RawValue;
            GameSystemRawId = roster.GameSystemLink.TargetId.RawValue;
            OriginProgramVersion = roster.OriginProgramVersion;
            PointsValue = roster.PointCost;
            PointsLimit = roster.PointsLimit;
        }

        public string GameSystemRawId { get; }

        public string Name { get; }

        public string OriginProgramVersion { get; private set; }

        public decimal PointsLimit { get; }

        public decimal PointsValue { get; }

        public string RawId { get; }

        public static RosterInfo CreateFromStream(Stream stream)
        {
            var reader = XmlReader.Create(stream);
            var attributeDict = new Dictionary<string, string>();
            reader.MoveToContent();
            while (reader.MoveToNextAttribute())
            {
                attributeDict[reader.Name] = reader.Value;
            }
            return new RosterInfo(
                attributeDict["name"],
                attributeDict["id"],
                attributeDict["gameSystemId"],
                attributeDict["battleScribeVersion"],
                decimal.Parse(attributeDict["points"]),
                decimal.Parse(attributeDict["pointsLimit"]));
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
                return false;
            if (ReferenceEquals(this, obj))
                return true;
            var other = obj as RosterInfo;
            if (other == null)
                return false;
            return RawId.Equals(other.RawId)
                   && GameSystemRawId.Equals(other.GameSystemRawId);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hash = 0;
                hash = hash*397 + RawId.GetHashCode();
                hash = hash*397 + GameSystemRawId.GetHashCode();
                return hash;
            }
        }

        public override string ToString() => $"Roster \"{Name}\" @ {PointsValue}/{PointsLimit}pts";
    }
}
