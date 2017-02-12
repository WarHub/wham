namespace WarHub.Armoury.Model.BattleScribeXml
{
    using System.Xml.Serialization;

    public enum ConstraintKind
    {
        [XmlEnum("min")] Min,
        [XmlEnum("max")] Max
    }
}