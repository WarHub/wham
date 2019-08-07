using System.Collections.Immutable;
using System.Xml.Serialization;

namespace WarHub.ArmouryModel.Source
{
    [WhamNodeCore]
    [XmlRoot(
        XmlInformation.RootElementNames.DataIndex,
        Namespace = XmlInformation.Namespaces.DataIndexXmlns,
        IsNullable = false)]
    public partial class DataIndexCore
    {
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
    }
}
