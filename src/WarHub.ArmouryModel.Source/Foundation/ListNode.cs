using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;

namespace WarHub.ArmouryModel.Source
{
    /// <summary>
    /// An abstract base class for source node that represents
    /// a set of other nodes of the same type.
    /// </summary>
    /// <typeparam name="TChild">Type of all child nodes.</typeparam>
    [DebuggerDisplay("Count = {" + nameof(Count) + "}, ElementKind = {" + nameof(ElementKind) + "}")]
    public abstract class ListNode<TChild> : SourceNode, IListNode, IReadOnlyList<TChild>
        where TChild : SourceNode
    {
        protected ListNode(SourceNode? parent) : base(parent)
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
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        public sealed override bool IsList => true;

        /// <summary>
        /// Gets a list of this node's child elements.
        /// </summary>
        public abstract NodeList<TChild> NodeList { get; }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
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
            return NodeList
                .Select((node, i) => new ChildInfo(i.ToString("D", CultureInfo.InvariantCulture), node));
        }

        /// <summary>
        /// Creates a new list node of the same type, but with a new set
        /// of child nodes.
        /// </summary>
        /// <param name="nodes">The nodes to become children of the new node.</param>
        /// <returns>The newly created <see cref="ListNode{TChild}"/>,
        /// being of the same type as this instance.</returns>
        public abstract ListNode<TChild> WithNodes(NodeList<TChild> nodes);

        public NodeList<TChild>.Enumerator GetEnumerator() => NodeList.GetEnumerator();

        IEnumerator<TChild> IEnumerable<TChild>.GetEnumerator() =>
            ((IEnumerable<TChild>)NodeList).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() =>
            ((IEnumerable)NodeList).GetEnumerator();

        protected internal override int ChildrenCount => NodeList.Count;

        protected internal override SourceNode GetChild(int index) => NodeList[index];
    }
}
