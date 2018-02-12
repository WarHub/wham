using System.Xml.Serialization;

namespace WarHub.ArmouryModel.Source
{
    [WhamNodeCore]
    public abstract partial class CostBaseCore
    {
        [XmlAttribute("name")]
        public string Name { get; }

        [XmlAttribute("costTypeId")]
        public string CostTypeId { get; }

        [XmlAttribute("value")]
        public decimal Value { get; }
    }
}
