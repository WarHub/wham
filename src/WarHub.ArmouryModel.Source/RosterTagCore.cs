using System.Xml.Serialization;

namespace WarHub.ArmouryModel.Source
{
    [WhamNodeCore]
    [XmlType("tag")]
    public sealed partial class RosterTagCore
    {
        [XmlAttribute("id")]
        public string? Id { get; }

        [XmlAttribute("name")]
        public string? Name { get; }
    }
}
