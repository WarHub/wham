using System.Xml.Serialization;

namespace WarHub.ArmouryModel.Source
{
    public enum ConstraintKind
    {
        [XmlEnum("min")]
        Minimum,

        [XmlEnum("max")]
        Maximum
    }
}
