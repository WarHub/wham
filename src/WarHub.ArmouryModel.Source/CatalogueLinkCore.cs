using System.Xml.Serialization;

namespace WarHub.ArmouryModel.Source
{
    [WhamNodeCore]
    [XmlType("catalogueLink")]
    public partial class CatalogueLinkCore
    {
        [XmlAttribute("id")]
        public string Id { get; }

        [XmlAttribute("name")]
        public string Name { get; }

        [XmlAttribute("targetId")]
        public string TargetId { get; }

        [XmlAttribute("type")]
        public CatalogueLinkKind Type { get; }

        [XmlAttribute("importRootEntries")]
        public bool ImportRootEntries { get; }
    }
}
