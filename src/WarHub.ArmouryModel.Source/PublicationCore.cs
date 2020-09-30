using System.Xml.Serialization;

namespace WarHub.ArmouryModel.Source
{
    [WhamNodeCore]
    [XmlType("publication")]
    public sealed partial record PublicationCore : CommentableCore
    {
        [XmlAttribute("id")]
        public string? Id { get; init; }

        [XmlAttribute("name")]
        public string? Name { get; init; }

        [XmlAttribute("shortName")]
        public string? ShortName { get; init; }

        [XmlAttribute("publisher")]
        public string? Publisher { get; init; }

        [XmlAttribute("publicationDate")]
        public string? PublicationDate { get; init; }

        [XmlAttribute("publisherUrl")]
        public string? PublisherUrl { get; init; }
    }
}
