using System.Xml.Serialization;

namespace WarHub.ArmouryModel.Source
{
    [WhamNodeCore]
    [XmlRoot("gameSystem", Namespace = GamesystemXmlNamespace, IsNullable = false)]
    public partial class GamesystemCore : CatalogueBaseCore
    {
        public const string GamesystemXmlNamespace = "http://www.battlescribe.net/schema/gameSystemSchema";

        public string DefaultXmlNamespace => GamesystemXmlNamespace;
    }
}
