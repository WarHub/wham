using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using MoreLinq;

namespace WarHub.ArmouryModel.Source
{
    public abstract class ListNode<TChild> : SourceNode, IListNode, IReadOnlyList<TChild>
        where TChild : SourceNode
    {
        protected ListNode(SourceNode parent) : base(parent)
        {
        }

        /// <summary>
        /// Gets count of this node's child elements.
        /// </summary>
        public int Count => NodeList.Count;

        /// <summary>
        /// Gets the kind of elements this list node contains.
        /// </summary>
        public abstract SourceKind ElementKind { get; }

        /// <summary>
        /// Gets <c>true</c> because this node is a list.
        /// </summary>
        public sealed override bool IsList => true;

        /// <summary>
        /// Gets a list of this node's child elements.
        /// </summary>
        public abstract NodeList<TChild> NodeList { get; }

        NodeList<SourceNode> IListNode.NodeList => NodeList;

        /// <summary>
        /// Gets a child element by <paramref name="index"/>.
        /// </summary>
        /// <param name="index">0-based index of element in this list.</param>
        /// <returns>Retrieved child element.</returns>
        public TChild this[int index] => NodeList[index];

        public override IEnumerable<SourceNode> Children() => NodeList;

        public override IEnumerable<ChildInfo> ChildrenInfos()
        {
            return NodeList.Index()
                .Select(x => new ChildInfo(x.Key.ToString("D", CultureInfo.InvariantCulture), x.Value));
        }

        public abstract ListNode<TChild> WithNodes(NodeList<TChild> nodes);

        public IEnumerator<TChild> GetEnumerator() => NodeList.GetEnumerator();

        IEnumerator<TChild> IEnumerable<TChild>.GetEnumerator() => NodeList.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => NodeList.GetEnumerator();

        protected internal override int ChildrenCount => NodeList.Count;

        protected internal override SourceNode GetChild(int index) => NodeList[index];
    }
}
