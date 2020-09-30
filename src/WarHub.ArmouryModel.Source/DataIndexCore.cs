using System.Collections.Immutable;
using System.Xml.Serialization;
using WarHub.ArmouryModel.Source.XmlFormat;

namespace WarHub.ArmouryModel.Source
{
    [WhamNodeCore]
    [XmlRoot(RootElementNames.DataIndex, Namespace = Namespaces.DataIndexXmlns, IsNullable = false)]
    public sealed partial record DataIndexCore
    {
        [XmlAttribute("battleScribeVersion")]
        public string? BattleScribeVersion { get; init; }

        [XmlAttribute("name")]
        public string? Name { get; init; }

        [XmlAttribute("indexUrl")]
        public string? IndexUrl { get; init; }

        [XmlArray("repositoryUrls")]
        public ImmutableArray<DataIndexRepositoryUrlCore> RepositoryUrls { get; init; } = ImmutableArray<DataIndexRepositoryUrlCore>.Empty;

        [XmlArray("dataIndexEntries")]
        public ImmutableArray<DataIndexEntryCore> DataIndexEntries { get; init; } = ImmutableArray<DataIndexEntryCore>.Empty;
    }
}
