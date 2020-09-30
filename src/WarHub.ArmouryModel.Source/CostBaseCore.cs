using System.Xml.Serialization;

namespace WarHub.ArmouryModel.Source
{
    [WhamNodeCore]
    public abstract partial record CostBaseCore
    {
        [XmlAttribute("name")]
        public string? Name { get; init; }

        [XmlAttribute("typeId")]
        public string? TypeId { get; init; }

        [XmlAttribute("value")]
        public decimal Value { get; init; }
    }
}
