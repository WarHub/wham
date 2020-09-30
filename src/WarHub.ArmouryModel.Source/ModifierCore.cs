using System.Xml.Serialization;

namespace WarHub.ArmouryModel.Source
{
    [WhamNodeCore]
    [XmlType("modifier")]
    public sealed partial record ModifierCore : ModifierBaseCore
    {
        [XmlAttribute("type")]
        public ModifierKind Type { get; init; }

        [XmlAttribute("field")]
        public string? Field { get; init; }

        [XmlAttribute("value")]
        public string? Value { get; init; }
    }
}
