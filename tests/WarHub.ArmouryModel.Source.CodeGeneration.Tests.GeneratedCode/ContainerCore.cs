using System.Collections.Immutable;
using System.Xml.Serialization;

namespace WarHub.ArmouryModel.Source
{
    [WhamNodeCore]
    [XmlRoot("container")]
    public sealed partial record ContainerCore
    {
        [XmlAttribute("id")]
        public string? Id { get; init; }

        [XmlAttribute("name")]
        public string? Name { get; init; }
        
        [XmlArray("items", Order = 0)]
        public ImmutableArray<ItemCore> Items { get; init; } = ImmutableArray<ItemCore>.Empty;
    }
}
