using System.Xml.Serialization;

namespace WarHub.ArmouryModel.Source
{
    public enum ConditionKind
    {
        [XmlEnum("lessThan")]
        LessThan,

        [XmlEnum("greaterThan")]
        GreaterThan,

        [XmlEnum("equalTo")]
        EqualTo,

        [XmlEnum("notEqualTo")]
        NotEqualTo,

        [XmlEnum("atLeast")]
        AtLeast,

        [XmlEnum("atMost")]
        AtMost,

        [XmlEnum("instanceOf")]
        InstanceOf,

        [XmlEnum("notInstanceOf")]
        NotInstanceOf
    }
}
