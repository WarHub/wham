using System.Xml.Serialization;

namespace WarHub.ArmouryModel.Source
{
    [WhamNodeCore]
    [XmlType("infoLink")]
    public partial class InfoLinkCore : LinkBaseCore
    {
        [XmlAttribute("type")]
        public InfoLinkKind Type { get; }
    }
}
