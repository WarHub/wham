using System.Xml.Serialization;

namespace WarHub.ArmouryModel.Source
{
    public enum ConditionGroupKind
    {
        [XmlEnum("and")]
        And,

        [XmlEnum("or")]
        Or,

        // NewRecruit additions: boolean, numeric, and comparison group kinds.
        [XmlEnum("not")]
        Not,

        [XmlEnum("count")]
        Count,

        [XmlEnum("add")]
        Add,

        [XmlEnum("subtract")]
        Subtract,

        [XmlEnum("multiply")]
        Multiply,

        [XmlEnum("divide")]
        Divide,

        [XmlEnum("modulo")]
        Modulo,

        [XmlEnum("power")]
        Power,

        [XmlEnum("min")]
        Min,

        [XmlEnum("max")]
        Max,

        [XmlEnum("greater")]
        Greater,

        [XmlEnum("greaterOrEqual")]
        GreaterOrEqual,

        [XmlEnum("less")]
        Less,

        [XmlEnum("lessOrEqual")]
        LessOrEqual,

        [XmlEnum("equal")]
        Equal,

        [XmlEnum("notEqual")]
        NotEqual
    }
}
