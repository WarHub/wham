using System.Xml.Serialization;

namespace WarHub.ArmouryModel.Source
{
    [WhamNodeCore]
    [XmlType("item")]
    public partial class ItemCore
    {
        [XmlAttribute("id")]
        public string? Id { get; }

        [XmlAttribute("name")]
        public string? Name { get; }
    }
}
