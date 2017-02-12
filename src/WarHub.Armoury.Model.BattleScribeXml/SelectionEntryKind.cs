namespace WarHub.Armoury.Model.BattleScribeXml
{
    using System.Xml.Serialization;

    public enum SelectionEntryKind
    {
        [XmlEnum("upgrade")] Upgrade,
        [XmlEnum("model")] Model,
        [XmlEnum("unit")] Unit
    }
}