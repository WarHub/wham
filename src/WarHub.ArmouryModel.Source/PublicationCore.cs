using System.Xml.Serialization;

namespace WarHub.ArmouryModel.Source
{
    [WhamNodeCore]
    [XmlType("publication")]
    public sealed partial class PublicationCore : CommentableCore
    {
        [XmlAttribute("id")]
        public string? Id { get; }

        [XmlAttribute("name")]
        public string? Name { get; }

        [XmlAttribute("shortName")]
        public string? ShortName { get; }

        [XmlAttribute("publisher")]
        public string? Publisher { get; }

        [XmlAttribute("publicationDate")]
        public string? PublicationDate { get; }

        [XmlAttribute("publisherUrl")]
        public string? PublisherUrl { get; }
    }
}
