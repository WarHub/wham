using System.Xml.Serialization;

namespace WarHub.ArmouryModel.Source
{
    [WhamNodeCore]
    [XmlType("infoLink")]
    public sealed partial class InfoLinkCore : EntryBaseCore
    {
        [XmlAttribute("targetId")]
        public string? TargetId { get; }

        [XmlAttribute("type")]
        public InfoLinkKind Type { get; }
    }
}
