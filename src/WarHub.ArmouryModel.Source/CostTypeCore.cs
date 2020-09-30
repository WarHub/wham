using System.Xml.Serialization;

namespace WarHub.ArmouryModel.Source
{
    [WhamNodeCore]
    [XmlType("costType")]
    public sealed partial record CostTypeCore : CommentableCore
    {
        [XmlAttribute("id")]
        public string? Id { get; init; }

        [XmlAttribute("name")]
        public string? Name { get; init; }

        [XmlAttribute("defaultCostLimit")]
        public decimal DefaultCostLimit { get; init; }

        [XmlAttribute("hidden")]
        public bool Hidden { get; init; }
    }
}
