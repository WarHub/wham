// WarHub licenses this file to you under the MIT license.
// See LICENSE file in the project root for more information.

namespace WarHub.Armoury.Model.Repo
{
    using System.Collections.Generic;
    using System.IO;
    using System.Xml;

    public sealed class GameSystemInfo
    {
        public GameSystemInfo(
            string name,
            string rawId,
            uint revision,
            string originProgramVersion,
            string sourcebook,
            string authorName)
        {
            Name = name;
            RawId = rawId;
            Revision = revision;
            OriginProgramVersion = originProgramVersion;
            Sourcebook = sourcebook;
            AuthorName = authorName;
        }

        public GameSystemInfo(IGameSystem system)
        {
            Name = system.Name;
            RawId = system.Id.RawValue;
            Revision = system.Revision;
            OriginProgramVersion = system.OriginProgramVersion;
            Sourcebook = system.BookSources;
            AuthorName = system.Author.Name;
        }

        public string AuthorName { get; }

        public string Name { get; }

        public string OriginProgramVersion { get; private set; }

        public string RawId { get; }

        public uint Revision { get; }

        public string Sourcebook { get; private set; }

        public static GameSystemInfo CreateFromStream(Stream stream)
        {
            var reader = XmlReader.Create(stream);
            var attributeDict = new Dictionary<string, string>
            {
                ["books"] = string.Empty,
                ["authorName"] = string.Empty
            };
            reader.MoveToContent();
            while (reader.MoveToNextAttribute())
            {
                attributeDict[reader.Name] = reader.Value;
            }
            return new GameSystemInfo(
                attributeDict["name"],
                attributeDict["id"],
                uint.Parse(attributeDict["revision"]),
                attributeDict["battleScribeVersion"],
                attributeDict["books"],
                attributeDict["authorName"]);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
                return false;
            if (ReferenceEquals(this, obj))
                return true;
            var other = obj as GameSystemInfo;
            if (other == null)
                return false;
            return RawId.Equals(other.RawId)
                   && Revision.Equals(other.Revision);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hash = 0;
                hash = hash*397 + RawId.GetHashCode();
                hash = hash*397 + Revision.GetHashCode();
                return hash;
            }
        }

        public override string ToString()
        {
            return $"{Name} (v{Revision} by {AuthorName})";
        }
    }
}
