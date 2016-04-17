namespace WarHub.Armoury.Model.EntryTree
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public sealed class TreeRoot : INode
    {
        public TreeRoot(IEntry entry)
        {
            if (entry == null) throw new ArgumentNullException(nameof(entry));
            GroupNodes = entry.GetGroupLinkPairs().Select(pair => GroupNode.Create(pair, this)).ToArray();
            EntryNodes = entry.GetEntryLinkPairs().Select(pair => EntryNode.Create(pair, this)).ToArray();
        }

        public TreeRoot(ICatalogue catalogue, ICategory category)
        {
            if (catalogue == null) throw new ArgumentNullException(nameof(catalogue));
            if (category == null) throw new ArgumentNullException(nameof(category));
            GroupNodes = new IGroupNode[0];
            EntryNodes =
                catalogue.GetEntryLinkPairs()
                    .Where(pair => category.IdValueEquals(pair.CategoryId.Value))
                    .Select(pair => EntryNode.Create(pair, this))
                    .ToArray();
        }

        IEntryNode INode.AsEntryNode
        {
            get { throw new NotSupportedException($"Can't get {nameof(TreeRoot)} as {nameof(IEntryNode)}."); }
        }

        IGroupNode INode.AsGroupNode
        {
            get { throw new NotSupportedException($"Can't get {nameof(TreeRoot)} as {nameof(IGroupNode)}."); }
        }

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
        bool INode.IsForLinkGuid(Guid linkGuid) => false;
        bool INode.IsGroupNode => false;
        bool INode.IsLinkNode => false;
        bool INode.IsRoot => true;
        INode INode.Parent => this;

        public static TreeRoot Create(IEntry entry)
        {
            if (entry == null) throw new ArgumentNullException(nameof(entry));
            return new TreeRoot(entry);
        }

        public static TreeRoot Create(ICatalogue catalogue, ICategory category)
        {
            if (catalogue == null) throw new ArgumentNullException(nameof(catalogue));
            if (category == null) throw new ArgumentNullException(nameof(category));
            return new TreeRoot(catalogue, category);
        }
    }
}
