using System.Xml.Serialization;

namespace WarHub.ArmouryModel.Source
{
    [WhamNodeCore]
    [XmlRoot("gameSystem", Namespace = GameSystemXmlNamespace, IsNullable = false)]
    public partial class GameSystemCore : CatalogueBaseCore
    {
        public const string GameSystemXmlNamespace = "http://www.battlescribe.net/schema/gameSystemSchema";

        public string DefaultXmlNamespace => GameSystemXmlNamespace;
    }
}
