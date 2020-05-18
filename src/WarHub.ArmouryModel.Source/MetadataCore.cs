using System.Xml.Serialization;

namespace WarHub.ArmouryModel.Source
{
    /// <summary>
    /// Describes metadata of an item, further along referred to as target item.
    /// </summary>
    [WhamNodeCore]
    public sealed partial class MetadataCore
    {
        /// <summary>
        /// Gets an identifier of the target item described by this meta within some collection.
        /// </summary>
        public string? Identifier { get; }

        /// <summary>
        /// Gets an identifier of an item preceding the target item.
        /// </summary>
        public string? PrevIdentifier { get; }

        /// <summary>
        /// Gets sequence number of the target item within some collection, or null if not specified.
        /// </summary>
        public int? Sequence { get; }
    }
}
