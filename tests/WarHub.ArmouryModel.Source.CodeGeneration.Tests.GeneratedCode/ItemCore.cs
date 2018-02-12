using System.Xml.Serialization;

namespace WarHub.ArmouryModel.Source.CodeGeneration.Tests.GeneratedCode
{
    [WhamNodeCore]
    [XmlType("item")]
    public partial class ItemCore
    {
        [XmlAttribute("id")]
        public string Id { get; }

        [XmlAttribute("name")]
        public string Name { get; }

        //public static CharacteristicType EmptyInstance { get; } = new Builder
        //{
        //    Id = Guid.Empty.ToString()
        //}.ToImmutable();
    }
}
