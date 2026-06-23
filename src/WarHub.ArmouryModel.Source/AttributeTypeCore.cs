using System.Xml.Serialization;

namespace WarHub.ArmouryModel.Source
{
    /// <summary>
    /// NewRecruit addition: an attribute type on a profile type (parallel to
    /// <see cref="CharacteristicTypeCore" />), used to carry export-only data. Not present in
    /// original BattleScribe v2.03.
    /// </summary>
    [WhamNodeCore]
    [XmlType("attributeType")]
    public sealed partial record AttributeTypeCore : CommentableCore
    {
        [XmlAttribute("id")]
        public string? Id { get; init; }

        [XmlAttribute("name")]
        public string? Name { get; init; }
    }
}
