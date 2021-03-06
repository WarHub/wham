﻿using System.Xml.Serialization;

namespace WarHub.ArmouryModel.Source
{
    /// <summary>
    /// Describes metadata of an item, further along referred to as target item.
    /// </summary>
    [WhamNodeCore]
    public sealed partial record MetadataCore
    {
        /// <summary>
        /// Gets an identifier of the target item described by this meta within some collection.
        /// </summary>
        public string? Identifier { get; init; }

        /// <summary>
        /// Gets an identifier of an item preceding the target item.
        /// </summary>
        public string? PrevIdentifier { get; init; }

        /// <summary>
        /// Gets sequence number of the target item within some collection, defaults to 0.
        /// </summary>
        public int Sequence { get; init; }
    }
}
