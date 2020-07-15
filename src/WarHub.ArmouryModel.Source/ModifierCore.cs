using System.Xml.Serialization;

namespace WarHub.ArmouryModel.Source
{
    [WhamNodeCore]
    [XmlType("modifier")]
    public sealed partial class ModifierCore : ModifierBaseCore
    {
        [XmlAttribute("type")]
        public ModifierKind Type { get; }

        [XmlAttribute("field")]
        public string? Field { get; }

        [XmlAttribute("value")]
        public string? Value { get; }
    }
}
