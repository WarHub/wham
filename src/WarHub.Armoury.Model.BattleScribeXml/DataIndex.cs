// WarHub licenses this file to you under the MIT license.
// See LICENSE file in the project root for more information.

namespace WarHub.Armoury.Model.BattleScribeXml
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Xml.Serialization;
    using Repo;

    [XmlRoot("dataIndex", Namespace = BsDataIndexXmlNamespace)]
    public sealed class DataIndex : IXmlProperties
    {
        public const string BsDataIndexXmlNamespace =
            "http://www.battlescribe.net/schema/dataIndexSchema";

        [XmlAttribute("battleScribeVersion")]
        public string BattleScribeVersion { get; set; }

        [XmlAttribute("name")]
        public string Name { get; set; }

        [XmlAttribute("indexUrl")]
        public string IndexUrl { get; set; }

        public string DefaultXmlNamespace => BsDataIndexXmlNamespace;

        [XmlArray("repositoryUrls")]
        [XmlArrayItem(ElementName = "repositoryUrl")]
        public List<string> RepositoryUrls { get; set; } = new List<string>(0);

        [XmlArray("dataIndexEntries")]
        public List<DataIndexEntry> DataIndexEntries { get; set; } = new List<DataIndexEntry>();

        public static DataIndex CreateFromSourceIndex(RemoteSourceDataIndex index)
        {
            return new DataIndex
            {
                BattleScribeVersion = index.OriginProgramVersion,
                Name = index.Name,
                IndexUrl = index.IndexUri.ToString(),
                DataIndexEntries = new List<DataIndexEntry>(
                    index.RemoteDataInfos.Select(x => DataIndexEntry.CreateFromInfo(x)))
            };
        }

        /// <summary>
        ///     Builds <see cref="RemoteSourceDataIndex" /> out of this object.
        /// </summary>
        /// <returns>Built index.</returns>
        /// <exception cref="NotSupportedException">When there are no repo addresses available in this object.</exception>
        public RemoteSourceDataIndex CreateSourceIndex()
        {
            var isIndexUrlNull = IndexUrl == null;
            if (isIndexUrlNull && (RepositoryUrls == null || RepositoryUrls.Count < 1))
            {
                throw new NotSupportedException($"Cannot create {nameof(RemoteSourceDataIndex)}"
                                                + $" without any repo address in {nameof(DataIndex)}.");
            }
            return new RemoteSourceDataIndex(DataIndexEntries.Select(x => x.CreateInfo()))
            {
                Name = Name,
                OriginProgramVersion = BattleScribeVersion,
                IndexUri = new Uri(isIndexUrlNull ? RepositoryUrls.First() : IndexUrl)
            };
        }
    }
}
