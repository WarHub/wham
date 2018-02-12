using System.Xml.Serialization;

namespace WarHub.ArmouryModel.Source
{
    public enum InfoLinkKind
    {
        [XmlEnum("profile")]
        Profile,

        [XmlEnum("rule")]
        Rule
    }
}
