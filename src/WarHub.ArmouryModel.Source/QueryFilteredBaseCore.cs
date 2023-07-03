using System.Xml.Serialization;

namespace WarHub.ArmouryModel.Source
{
    [WhamNodeCore]
    public abstract partial record QueryFilteredBaseCore : QueryBaseCore
    {
        /// <summary>
        /// Changes the query to filter by this value.
        /// </summary>
        [XmlAttribute("childId")]
        public abstract string? ChildId { get; init; }
    }
}
