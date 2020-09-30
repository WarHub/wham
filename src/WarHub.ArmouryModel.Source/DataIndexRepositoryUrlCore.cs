using System.Xml.Serialization;

namespace WarHub.ArmouryModel.Source
{
    [WhamNodeCore]
    [XmlType("repositoryUrl")]
    public sealed partial record DataIndexRepositoryUrlCore
    {
        [XmlText]
        public string? Value { get; init; }
    }
}
