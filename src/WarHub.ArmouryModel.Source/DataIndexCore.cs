using System.Collections.Immutable;
using System.Xml.Serialization;

namespace WarHub.ArmouryModel.Source
{
    [WhamNodeCore]
    [XmlRoot("dataIndex", Namespace = DataIndexXmlNamespace)]
    public partial class DataIndexCore
    {
        public const string DataIndexXmlNamespace = "http://www.battlescribe.net/schema/dataIndexSchema";

        [XmlAttribute("battleScribeVersion")]
        public string BattleScribeVersion { get; set; }

        [XmlAttribute("name")]
        public string Name { get; set; }

        [XmlAttribute("indexUrl")]
        public string IndexUrl { get; set; }

        [XmlArray("repositoryUrls")]
        public ImmutableArray<DataIndexRepositoryUrlCore> RepositoryUrls { get; }

        [XmlArray("dataIndexEntries")]
        public ImmutableArray<DataIndexEntryCore> DataIndexEntries { get; }

        public string DefaultXmlNamespace => DataIndexXmlNamespace;
    }
}
