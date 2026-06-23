using System.ComponentModel;
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

        // NewRecruit additions. DefaultValue keeps them out of serialized output unless set, so
        // original BattleScribe data (which has neither attribute) round-trips byte-identically.
        [XmlAttribute("negative")]
        [DefaultValue(false)]
        public bool Negative { get; init; }

        [XmlAttribute("automatic")]
        [DefaultValue(false)]
        public bool Automatic { get; init; }

        [XmlAttribute("message")]
        public string? Message { get; init; }
    }
}
