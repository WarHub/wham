namespace WarHub.Armoury.Model.EntryTree
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;

    [DebuggerDisplay("{Entry.Name}")]
    public sealed class EntryNode : IEntryNode
    {
        public EntryNode(EntryLinkPair pair, INode parent)
        {
            if (pair == null) throw new ArgumentNullException(nameof(pair));
            if (parent == null) throw new ArgumentNullException(nameof(parent));
            EntryLinkPair = pair;
            Parent = parent;
        }

        public IEntry Entry => EntryLinkPair.Entry;
        public EntryLinkPair EntryLinkPair { get; }
        public IEntryLink Link => EntryLinkPair.Link;
        IEntryNode INode.AsEntryNode => this;

        IGroupNode INode.AsGroupNode
        {
            get { throw new NotSupportedException($"Can't get {nameof(IEntryNode)} as {nameof(IGroupNode)}."); }
        }

        IEnumerable<INode> INode.Children => Enumerable.Empty<INode>();
        IEnumerable<IEntryNode> INode.EntryNodes => Enumerable.Empty<IEntryNode>();
        IEnumerable<IGroupNode> INode.GroupNodes => Enumerable.Empty<IGroupNode>();
        bool INode.IsEntryNode => true;

        /// <summary>
        ///     Checks whether this node handles given link by comparing their guids.
        /// </summary>
        /// <param name="linkGuid">Guid of link to be handled.</param>
        /// <returns>True if this node is for link with given guid.</returns>
        public bool IsForLinkGuid(Guid linkGuid) => IsLinkNode && Link.IdValueEquals(linkGuid);

        bool INode.IsGroupNode => false;

        /// <summary>
        ///     Gets if this node is based on link.
        /// </summary>
        public bool IsLinkNode => EntryLinkPair.HasLink;

        bool INode.IsRoot => false;

        /// <summary>
        ///     Gets parent node. Root returns itself.
        /// </summary>
        public INode Parent { get; }

        public static EntryNode Create(EntryLinkPair pair, INode parent)
        {
            if (pair == null) throw new ArgumentNullException(nameof(pair));
            if (parent == null) throw new ArgumentNullException(nameof(parent));
            return new EntryNode(pair, parent);
        }
    }
}
