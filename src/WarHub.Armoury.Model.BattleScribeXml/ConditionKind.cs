namespace WarHub.Armoury.Model.BattleScribeXml
{
    using System.Xml.Serialization;

    public enum ConditionKind
    {
        [XmlEnum("lessThan")] LessThan,
        [XmlEnum("greaterThan")] GreaterThan,
        [XmlEnum("equalTo")] EqualTo,
        [XmlEnum("notEqualTo")] NotEqualTo,
        [XmlEnum("atLeast")] AtLeast,
        [XmlEnum("atMost")] AtMost,
        [XmlEnum("instanceOf")] InstanceOf,
        [XmlEnum("notInstanceOf")] NotInstanceOf
    }
}