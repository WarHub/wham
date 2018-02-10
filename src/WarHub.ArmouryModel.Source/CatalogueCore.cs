using System.Xml.Serialization;

namespace WarHub.ArmouryModel.Source
{
    [WhamNodeCore]
    [XmlRoot("catalogue", Namespace = CatalogueXmlNamespace, IsNullable = false)]
    public partial class CatalogueCore : CatalogueBaseCore
    {
        public const string CatalogueXmlNamespace = "http://www.battlescribe.net/schema/catalogueSchema";

        [XmlAttribute("gameSystemId")]
        public string GamesystemId { get; }

        [XmlAttribute("gameSystemRevision")]
        public int GamesystemRevision { get; }

        public string DefaultXmlNamespace => CatalogueXmlNamespace;
    }
}
