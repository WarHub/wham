using System.Xml.Serialization;

namespace WarHub.ArmouryModel.Source
{
    [WhamNodeCore]
    public abstract partial class LinkBaseCore : EntryBaseCore
    {
        [XmlAttribute("targetId")]
        public string TargetId { get; }
    }
}
