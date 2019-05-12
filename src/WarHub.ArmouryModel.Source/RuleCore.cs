using System.Xml.Serialization;

namespace WarHub.ArmouryModel.Source
{
    [WhamNodeCore]
    [XmlType("rule")]
    public partial class RuleCore : EntryBaseCore
    {
        [XmlElement("description")]
        public string Description { get; }
    }
}
