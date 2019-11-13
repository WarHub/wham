using System.Xml.Serialization;

namespace WarHub.ArmouryModel.Source
{
    [WhamNodeCore]
    [XmlType("rule")]
    public sealed partial class RuleCore : EntryBaseCore
    {
        [XmlElement("description")]
        public string Description { get; }
    }
}
