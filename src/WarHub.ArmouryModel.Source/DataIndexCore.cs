﻿using System.Collections.Immutable;
using System.Xml.Serialization;

namespace WarHub.ArmouryModel.Source
{
    [WhamNodeCore]
    [XmlRoot("dataIndex", Namespace = DataIndexXmlNamespace)]
    public partial class DataIndexCore
    {
        public const string DataIndexXmlNamespace = "http://www.battlescribe.net/schema/dataIndexSchema";

        [XmlAttribute("battleScribeVersion")]
        public string BattleScribeVersion { get; }

        [XmlAttribute("name")]
        public string Name { get; }

        [XmlAttribute("indexUrl")]
        public string IndexUrl { get; }

        [XmlArray("repositoryUrls")]
        public ImmutableArray<DataIndexRepositoryUrlCore> RepositoryUrls { get; }

        [XmlArray("dataIndexEntries")]
        public ImmutableArray<DataIndexEntryCore> DataIndexEntries { get; }

        public string DefaultXmlNamespace => DataIndexXmlNamespace;
    }
}
