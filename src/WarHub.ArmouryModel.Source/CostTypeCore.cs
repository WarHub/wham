using System.Xml.Serialization;

namespace WarHub.ArmouryModel.Source
{
    [WhamNodeCore]
    [XmlType("costType")]
    public sealed partial class CostTypeCore : CommentableCore
    {
        [XmlAttribute("id")]
        public string? Id { get; }

        [XmlAttribute("name")]
        public string? Name { get; }

        [XmlAttribute("defaultCostLimit")]
        public decimal DefaultCostLimit { get; }

        [XmlAttribute("hidden")]
        public bool Hidden { get; }
    }
}
