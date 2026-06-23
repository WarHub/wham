using System.Xml.Serialization;

namespace WarHub.ArmouryModel.Source
{
    /// <summary>
    /// NewRecruit addition: an association relates a selection to a number (min/max) of other
    /// selections resolved by a query (scope/field/childId), inheriting the query attributes from
    /// <see cref="QueryBaseCore" />. Not present in original BattleScribe v2.03.
    /// </summary>
    [WhamNodeCore]
    [XmlType("association")]
    public sealed partial record AssociationCore : QueryBaseCore
    {
        [XmlAttribute("id")]
        public string? Id { get; init; }

        [XmlAttribute("name")]
        public string? Name { get; init; }

        [XmlAttribute("min")]
        public int Min { get; init; }

        [XmlAttribute("max")]
        public int Max { get; init; }

        [XmlAttribute("childId")]
        public string? ChildId { get; init; }
    }
}
