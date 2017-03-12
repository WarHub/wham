// WarHub licenses this file to you under the MIT license.
// See LICENSE file in the project root for more information.

namespace WarHub.Armoury.Model.Source
{
    using System.Collections;
    using System.Collections.Generic;
    public struct SourceNodeList<TNode> : IReadOnlyList<TNode> where TNode : SourceNode
    {
        private readonly SourceNode _node;

        public SourceNodeList(SourceNode node)
        {
            _node = node;
        }

        public TNode this[int index] => (TNode)_node.GetSlotNode(index);

        public int Count => _node.GetSlotCount();

        public IEnumerator<TNode> GetEnumerator()
        {
            for (int i = 0; i < Count; i++)
            {
                yield return this[i];
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            for (int i = 0; i < Count; i++)
            {
                yield return this[i];
            }
        }
    }
}
