using System.Xml.Serialization;

namespace WarHub.ArmouryModel.Source
{
    [WhamNodeCore]
    [XmlType("constraint")]
    public sealed partial record ConstraintCore : QueryBaseCore
    {
        [XmlAttribute("id")]
        public string? Id { get; init; }

        [XmlAttribute("type")]
        public ConstraintKind Type { get; init; }
    }
}
