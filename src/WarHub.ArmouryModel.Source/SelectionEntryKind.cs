using System.Xml.Serialization;

namespace WarHub.ArmouryModel.Source
{
    public enum SelectionEntryKind
    {
        [XmlEnum("upgrade")]
        Upgrade,

        [XmlEnum("model")]
        Model,

        [XmlEnum("unit")]
        Unit
    }
}
