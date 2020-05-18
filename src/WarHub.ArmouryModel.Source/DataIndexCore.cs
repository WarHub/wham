using System.Collections.Immutable;
using System.Xml.Serialization;
using WarHub.ArmouryModel.Source.XmlFormat;

namespace WarHub.ArmouryModel.Source
{
    [WhamNodeCore]
    [XmlRoot(RootElementNames.DataIndex, Namespace = Namespaces.DataIndexXmlns, IsNullable = false)]
    public sealed partial class DataIndexCore
    {
        [XmlAttribute("battleScribeVersion")]
        public string? BattleScribeVersion { get; }

        [XmlAttribute("name")]
        public string Name { get; }

        [XmlAttribute("indexUrl")]
        public string? IndexUrl { get; }

        [XmlArray("repositoryUrls")]
        public ImmutableArray<DataIndexRepositoryUrlCore> RepositoryUrls { get; }

        [XmlArray("dataIndexEntries")]
        public ImmutableArray<DataIndexEntryCore> DataIndexEntries { get; }
    }
}
