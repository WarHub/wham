namespace WarHub.Armoury.Model.BattleScribeXml
{
    using System.Xml.Serialization;

    [XmlRoot("gameSystem", Namespace = "http://www.battlescribe.net/schema/catalogueSchema", IsNullable = false)]
    public class GameSystem : Datablob
    {
    }
}