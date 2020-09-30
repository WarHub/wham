using System.Xml.Serialization;

namespace WarHub.ArmouryModel.Source
{
    [WhamNodeCore]
    [XmlType("infoLink")]
    public sealed partial record InfoLinkCore : EntryBaseCore
    {
        [XmlAttribute("targetId")]
        public string? TargetId { get; init; }

        [XmlAttribute("type")]
        public InfoLinkKind Type { get; init; }
    }
}
