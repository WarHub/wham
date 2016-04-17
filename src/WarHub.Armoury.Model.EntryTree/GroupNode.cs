// WarHub licenses this file to you under the MIT license.
// See LICENSE file in the project root for more information.

namespace WarHub.Armoury.Model.EntryTree
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;

    [DebuggerDisplay("{Group.Name}")]
    public sealed class GroupNode : IGroupNode
    {
        public GroupNode(GroupLinkPair pair, INode parent)
        {
            if (pair == null)
                throw new ArgumentNullException(nameof(pair));
            if (parent == null)
                throw new ArgumentNullException(nameof(parent));
            GroupLinkPair = pair;
            Parent = parent;
            GroupNodes = Group.GetGroupLinkPairs().Select(groupPair => Create(groupPair, this)).ToArray();
            EntryNodes = Group.GetEntryLinkPairs().Select(entryPair => EntryNode.Create(entryPair, this)).ToArray();
        }

        public IGroup Group => GroupLinkPair.Group;

        public GroupLinkPair GroupLinkPair { get; }

        public IGroupLink Link => GroupLinkPair.Link;

        IEntryNode INode.AsEntryNode
        {
            get { throw new NotSupportedException($"Can't get {nameof(IGroupNode)} as {nameof(IEntryNode)}."); }
        }

        IGroupNode INode.AsGroupNode => this;

        /// <summary>
        ///     Enumerates all child nodes.
        /// </summary>
        public IEnumerable<INode> Children
        {
            get
            {
                foreach (var entryNode in EntryNodes)
                {
                    yield return entryNode;
                }
                foreach (var groupNode in GroupNodes)
                {
                    yield return groupNode;
                }
            }
        }

        /// <summary>
        ///     Enumerates all of this node's child entry nodes.
        /// </summary>
        public IEnumerable<IEntryNode> EntryNodes { get; }

        /// <summary>
        ///     Enumerates all of this node's child group nodes.
        /// </summary>
        public IEnumerable<IGroupNode> GroupNodes { get; }

        bool INode.IsEntryNode => false;

        /// <summary>
        ///     Checks whether this node handles given link by comparing their guids.
        /// </summary>
        /// <param name="linkGuid">Guid of link to be handled.</param>
        /// <returns>True if this node is for link with given guid.</returns>
        public bool IsForLinkGuid(Guid linkGuid) => IsLinkNode && Link.IdValueEquals(linkGuid);

        bool INode.IsGroupNode => true;

        /// <summary>
        ///     Gets if this node is based on link.
        /// </summary>
        public bool IsLinkNode => GroupLinkPair.HasLink;

        bool INode.IsRoot => false;

        /// <summary>
        ///     Gets parent node. Root returns itself.
        /// </summary>
        public INode Parent { get; }

        public static GroupNode Create(GroupLinkPair pair, INode parent)
        {
            if (pair == null)
                throw new ArgumentNullException(nameof(pair));
            if (parent == null)
                throw new ArgumentNullException(nameof(parent));
            return new GroupNode(pair, parent);
        }
    }
}
