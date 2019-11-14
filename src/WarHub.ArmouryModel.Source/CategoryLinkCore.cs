using System.Xml.Serialization;

namespace WarHub.ArmouryModel.Source
{
    [WhamNodeCore]
    [XmlType("categoryLink")]
    public sealed partial class CategoryLinkCore : ContainerEntryBaseCore
    {
        [XmlAttribute("targetId")]
        public string TargetId { get; }

        [XmlAttribute("primary")]
        public bool Primary { get; }
    }
}
