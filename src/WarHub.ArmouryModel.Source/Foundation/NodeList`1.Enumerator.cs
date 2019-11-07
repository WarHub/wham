using System.Collections;
using System.Collections.Generic;

namespace WarHub.ArmouryModel.Source
{
    public partial struct NodeList<TNode>
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1034:Nested types should not be visible", Justification = "Performance optimization for GetEnumerator needs this type as public.")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1815:Override equals and operator equals on value types", Justification = "It's an Enumerator. Doesn't need those.")]
        public struct Enumerator
        {
            private int index;
            private readonly NodeList<TNode> nodeList;

            internal Enumerator(NodeList<TNode> nodeList) =>
                (this.nodeList, index) = (nodeList, -1);

            public TNode Current => nodeList[index];

            public bool MoveNext() => ++index < nodeList.Count;
        }

        private sealed class EnumeratorObject : IEnumerator<TNode>
        {
            private int index;
            private readonly NodeList<TNode> nodeList;
            private static readonly EnumeratorObject emptyEnumerator = new EnumeratorObject(default);

            private EnumeratorObject(NodeList<TNode> nodeList) =>
                (this.nodeList, index) = (nodeList, -1);

            public TNode Current => nodeList[index];

            object IEnumerator.Current => Current;

            public void Dispose()
            {
                // no resources to dispose
            }

            public bool MoveNext() => ++index < nodeList.Count;

            void IEnumerator.Reset() => index = -1;

            internal static EnumeratorObject Create(NodeList<TNode> nodeList) =>
                nodeList.Count == 0 ? emptyEnumerator : new EnumeratorObject(nodeList);
        }
    }
}
