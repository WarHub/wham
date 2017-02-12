namespace WarHub.Armoury.Model.BattleScribeXml
{
    using System.Xml.Serialization;

    [XmlRoot("gameSystem", Namespace = GameSystemXmlNamespace, IsNullable = false)]
    public class GameSystem : Datablob, IXmlProperties
    {
        public const string GameSystemXmlNamespace = "http://www.battlescribe.net/schema/gameSystemSchema";

        public string DefaultXmlNamespace => GameSystemXmlNamespace;
    }
}