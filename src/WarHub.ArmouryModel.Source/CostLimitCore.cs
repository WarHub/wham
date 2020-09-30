using System.Xml.Serialization;

namespace WarHub.ArmouryModel.Source
{
    [WhamNodeCore]
    [XmlType("costLimit")]
    public sealed partial record CostLimitCore : CostBaseCore
    {
    }
}
