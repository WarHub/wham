// WarHub licenses this file to you under the MIT license.
// See LICENSE file in the project root for more information.

namespace WarHub.Armoury.Model.Repo
{
    using System.Xml.Serialization;

    /// <summary>
    ///     Type of data.
    /// </summary>
    public enum RemoteDataType
    {
        [XmlEnum("catalogue")] Catalogue,

        [XmlEnum("gamesystem")] GameSystem
    }

    /// <summary>
    ///     Describes single data entry in remote location.
    /// </summary>
    public class RemoteDataInfo
    {
        public RemoteDataInfo(
            string indexPathSuffix,
            string name,
            string originProgramVersion,
            string rawId,
            uint revision,
            RemoteDataType dataType)
        {
            IndexPathSuffix = indexPathSuffix;
            Name = name;
            OriginProgramVersion = originProgramVersion;
            RawId = rawId;
            Revision = revision;
            DataType = dataType;
        }

        public RemoteDataType DataType { get; }

        public string IndexPathSuffix { get; }

        public string Name { get; }

        public string OriginProgramVersion { get; }

        public string RawId { get; }

        public uint Revision { get; }
    }
}
