// WarHub licenses this file to you under the MIT license.
// See LICENSE file in the project root for more information.

namespace WarHub.Armoury.Model.Source
{
    public abstract class SourceNode
    {
        protected SourceNode(SourceTree tree)
        {
            Tree = tree;
        }

        protected SourceTree Tree { get; }

        public abstract SourceNode Parent { get; }

        public abstract SourceNodeKind NodeKind { get; }

        public abstract SourceNodeList<SourceNode> Children { get; }

        public abstract string ModelLanguage { get; }

        internal abstract int GetSlotCount();

        internal abstract SourceNode GetSlotNode(int index);
    }
}
