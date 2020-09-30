using System.Xml.Serialization;

namespace WarHub.ArmouryModel.Source
{
    [WhamNodeCore]
    [XmlType("rule")]
    public sealed partial record RuleCore : EntryBaseCore
    {
        [XmlElement("description")]
        public string? Description { get; init; }
    }
}
