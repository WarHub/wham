using System.Xml.Serialization;

namespace WarHub.ArmouryModel.Source
{
    [WhamNodeCore]
    [XmlType("class1")]
    public partial record Class1
    {
        public string? DemoProperty { get; init; }
    }
}
