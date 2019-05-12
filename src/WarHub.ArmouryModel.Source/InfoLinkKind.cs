using System.Xml.Serialization;

namespace WarHub.ArmouryModel.Source
{
    public enum InfoLinkKind
    {
        [XmlEnum("infoGroup")]
        InfoGroup,

        [XmlEnum("profile")]
        Profile,

        [XmlEnum("rule")]
        Rule
    }
}
