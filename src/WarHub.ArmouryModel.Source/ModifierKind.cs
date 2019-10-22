using System.Xml.Serialization;

namespace WarHub.ArmouryModel.Source
{
    public enum ModifierKind
    {
        [XmlEnum("set")]
        Set,

        [XmlEnum("increment")]
        Increment,

        [XmlEnum("decrement")]
        Decrement,

        [XmlEnum("append")]
        Append,

        [XmlEnum("add")]
        Add,

        [XmlEnum("remove")]
        Remove,

        [XmlEnum("set-primary")]
        SetPrimary,

        [XmlEnum("unset-primary")]
        UnsetPrimary
    }
}
