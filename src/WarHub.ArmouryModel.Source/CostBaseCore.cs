using System.Xml.Serialization;

namespace WarHub.ArmouryModel.Source
{
    [WhamNodeCore]
    public abstract partial class CostBaseCore
    {
        [XmlAttribute("name")]
        public string? Name { get; }

        [XmlAttribute("typeId")]
        public string? TypeId { get; }

        [XmlAttribute("value")]
        public decimal Value { get; }
    }
}
