namespace WarHub.Armoury.Model.BattleScribeXml
{
    using System.Xml.Serialization;

    public enum InfoLinkKind
    {
        [XmlEnum("profile")] Profile,
        [XmlEnum("rule")] Rule
    }
}