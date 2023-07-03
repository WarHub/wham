using System.Xml.Serialization;

namespace WarHub.ArmouryModel.Source
{
    [WhamNodeCore]
    [XmlType("catalogueLink")]
    public sealed partial record CatalogueLinkCore : CommentableCore
    {
        [XmlAttribute("id")]
        public string? Id { get; init; }

        [XmlAttribute("name")]
        public string? Name { get; init; }

        [XmlAttribute("targetId")]
        public string? TargetId { get; init; }

        [XmlAttribute("type")]
        public CatalogueLinkKind Type { get; init; }

        [XmlAttribute("importRootEntries")]
        public bool ImportRootEntries { get; init; }

        /// <inheritdoc />
        [XmlElement("comment")]
        public override string? Comment { get; init; }
    }
}
