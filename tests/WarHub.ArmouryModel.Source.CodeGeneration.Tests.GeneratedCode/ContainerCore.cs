using System.Collections.Immutable;
using System.Xml.Serialization;

namespace WarHub.ArmouryModel.Source.CodeGeneration.Tests.GeneratedCode
{
    [WhamNodeCore]
    [XmlType("container")]
    public partial class ContainerCore
    {
        [XmlAttribute("id")]
        public string Id { get; }

        [XmlAttribute("name")]
        public string Name { get; }

        [XmlArray("items", Order = 0)]
        public ImmutableArray<ItemCore> Items { get; }
    }
}
