using System.Xml.Serialization;

namespace WarHub.ArmouryModel.Source
{
    public enum ConditionGroupKind
    {
        [XmlEnum("and")]
        And,

        [XmlEnum("or")]
        Or
    }
}
