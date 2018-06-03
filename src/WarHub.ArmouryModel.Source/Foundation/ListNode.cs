using System.Collections;
using System.Collections.Generic;

namespace WarHub.ArmouryModel.Source
{
    public abstract class ListNode<TChild> : SourceNode, IReadOnlyList<TChild>
        where TChild : SourceNode
    {
        public ListNode(NodeList<TChild> nodeList, SourceNode parent) : base(null, parent)
        {
            NodeList = nodeList;
        }

        public sealed override bool IsList => true;

        public NodeList<TChild> NodeList { get; }

        public TChild this[int index] => (TChild)GetChild(index);

        protected internal override int ChildrenCount => NodeList.Count;

        public int Count => NodeList.Count;

        protected internal override SourceNode GetChild(int index) => NodeList[index];

        public IEnumerator<TChild> GetEnumerator() => NodeList.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
