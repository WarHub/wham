namespace WarHub.Armoury.Model.EntryTree
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    ///     Common interface for root and other nodes of entry/group tree. Provides enumeration of various kinds
    ///     (group/entry/links to both), as well as some details of what kind of node it is.
    /// </summary>
    public interface INode
    {
        /// <summary>
        ///     Gets this node as <see cref="IEntryNode" /> without casting.
        /// </summary>
        /// <exception cref="NotSupportedException">When it's not <see cref="IEntryNode" />.</exception>
        IEntryNode AsEntryNode { get; }

        /// <summary>
        ///     Gets this node as <see cref="IGroupNode" /> without casting.
        /// </summary>
        /// <exception cref="NotSupportedException">When it's not <see cref="IGroupNode" />.</exception>
        IGroupNode AsGroupNode { get; }

        /// <summary>
        ///     Enumerates all child nodes.
        /// </summary>
        IEnumerable<INode> Children { get; }

        /// <summary>
        ///     Enumerates all of this node's child entry nodes.
        /// </summary>
        IEnumerable<IEntryNode> EntryNodes { get; }

        /// <summary>
        ///     Enumerates all of this node's child group nodes.
        /// </summary>
        IEnumerable<IGroupNode> GroupNodes { get; }

        /// <summary>
        ///     Gets if this node is <see cref="IEntryNode" />.
        /// </summary>
        bool IsEntryNode { get; }

        /// <summary>
        ///     Gets if this node is <see cref="IGroupNode" />.
        /// </summary>
        bool IsGroupNode { get; }

        /// <summary>
        ///     Gets if this node is based on link.
        /// </summary>
        bool IsLinkNode { get; }

        /// <summary>
        ///     Gets whether this node is tree's root.
        /// </summary>
        bool IsRoot { get; }

        /// <summary>
        ///     Gets parent node. Root returns itself.
        /// </summary>
        INode Parent { get; }

        /// <summary>
        ///     Checks whether this node handles given link by comparing their guids.
        /// </summary>
        /// <param name="linkGuid">Guid of link to be handled.</param>
        /// <returns>True if this node is for link with given guid.</returns>
        bool IsForLinkGuid(Guid linkGuid);
    }
}
