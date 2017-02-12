namespace WarHub.Armoury.Model.BattleScribeXml
{
    using System.Xml.Serialization;

    public enum ConditionGroupKind
    {
        [XmlEnum("and")] And,
        [XmlEnum("or")] Or
    }
}