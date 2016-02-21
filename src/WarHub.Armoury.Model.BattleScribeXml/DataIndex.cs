// WarHub licenses this file to you under the MIT license.
// See LICENSE file in the project root for more information.

namespace WarHub.Armoury.Model.BattleScribeXml
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Xml.Serialization;
    using Repo;

    [XmlRoot("dataIndex", Namespace = BSDataIndexXMLNamespace)]
    public sealed class DataIndex : IXmlProperties, INamed
    {
        private const string BSDataIndexXMLNamespace =
            "http://www.battlescribe.net/schema/dataIndexSchema";

        [XmlAttribute("battleScribeVersion")]
        public string BattleScribeVersion { get; set; }

        [XmlAttribute("name")]
        public string Name { get; set; }

        [XmlAttribute("indexUrl")]
        public string IndexUrl { get; set; }

        public string DefaultXmlNamespace => BSDataIndexXMLNamespace;

        [XmlArray("repositoryUrls")]
        [XmlArrayItem(ElementName = "repositoryUrl")]
        public List<string> RepositoryUrls { get; set; } = new List<string>(0);

        [XmlArray("dataIndexEntries")]
        public List<DataIndexEntry> DataIndexEntries { get; set; } = new List<DataIndexEntry>();

        public static DataIndex CreateFromSourceIndex(RemoteDataSourceIndex index)
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
        ///     Builds <see cref="RemoteDataSourceIndex" /> out of this object.
        /// </summary>
        /// <returns>Built index.</returns>
        /// <exception cref="NotSupportedException">When there are no repo addresses available in this object.</exception>
        public RemoteDataSourceIndex CreateSourceIndex()
        {
            var isIndexUrlNull = IndexUrl == null;
            if (isIndexUrlNull && (RepositoryUrls == null || RepositoryUrls.Count < 1))
            {
                throw new NotSupportedException($"Cannot create {nameof(RemoteDataSourceIndex)}"
                                                + $" without any repo address in {nameof(DataIndex)}.");
            }
            return new RemoteDataSourceIndex(DataIndexEntries.Select(x => x.CreateInfo()))
            {
                Name = Name,
                OriginProgramVersion = BattleScribeVersion,
                IndexUri = new Uri(isIndexUrlNull ? RepositoryUrls.First() : IndexUrl)
            };
        }
    }
}
