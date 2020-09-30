using System.Xml.Serialization;

namespace WarHub.ArmouryModel.Source
{
    [WhamNodeCore]
    [XmlType("categoryLink")]
    public sealed partial record CategoryLinkCore : ContainerEntryBaseCore
    {
        [XmlAttribute("targetId")]
        public string? TargetId { get; init; }

        [XmlAttribute("primary")]
        public bool Primary { get; init; }
    }
}
