namespace WarHub.Armoury.Model.BattleScribeXml
{
    using System.Xml.Serialization;

    public enum ModifierKind
    {
        [XmlEnum("set")] Set,
        [XmlEnum("increment")] Increment,
        [XmlEnum("decrement")] Decrement,
        [XmlEnum("append")] Append
    }
}