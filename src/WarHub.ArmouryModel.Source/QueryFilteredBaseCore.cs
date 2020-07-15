using System.Xml.Serialization;

namespace WarHub.ArmouryModel.Source
{
    [WhamNodeCore]
    public abstract partial class QueryFilteredBaseCore : QueryBaseCore
    {
        /// <summary>
        /// Changes the query to filter by this value.
        /// </summary>
        [XmlAttribute("childId")]
        public string? ChildId { get; }
    }
}
