using System;
using System.Collections.Generic;
using System.Linq;

namespace WarHub.ArmouryModel.Source
{
    public abstract class SourceNode
    {
        public SourceNode(NodeCore core, SourceNode parent)
        {
            Core = core;
            Parent = parent;
        }

        protected SourceNode Parent { get; }

        // TODO SourceTree?

        internal NodeCore Core { get; }

        public IEnumerable<SourceNode> Ancestors()
        {
            return AncestorsCore(includeSelf: false);
        }

        public IEnumerable<SourceNode> AncestorsAndSelf()
        {
            return AncestorsCore(includeSelf: true);
        }

        public IEnumerable<SourceNode> Children()
        {
            for (int i = 0; i < ChildrenCount; i++)
            {
                yield return GetChild(i);
            }
        }

        public bool Contains(SourceNode node)
        {
            while ((node = node?.Parent) != null)
            {
                if (node == this)
                {
                    return true;
                }
            }
            return false;
        }

        public IEnumerable<SourceNode> Descendants(Func<SourceNode, bool> descendIntoChildren = null)
        {
            return DescendantsCore(descendIntoChildren, includeSelf: false);
        }

        public IEnumerable<SourceNode> DescendantsAndSelf(Func<SourceNode, bool> descendIntoChildren = null)
        {
            return DescendantsCore(descendIntoChildren, includeSelf: true);
        }

        public TNode FirstAncestorOrSelf<TNode>(Func<TNode, bool> predicate) where TNode : class
        {
            var node = this;
            while (node != null)
            {
                if (node is TNode typedNode && predicate(typedNode))
                {
                    return typedNode;
                }
                node = node.Parent;
            }
            return null;
        }

        private IEnumerable<SourceNode> AncestorsCore(bool includeSelf)
        {
            if (includeSelf)
            {
                yield return this;
            }
            var node = this;
            while ((node = node.Parent) != null)
            {
                yield return node;
            }
        }

        private IEnumerable<SourceNode> DescendantsCore(Func<SourceNode, bool> descendIntoChildren, bool includeSelf)
        {
            if (includeSelf)
            {
                yield return this;
            }
            if (ChildrenCount == 0)
            {
                yield break;
            }
            var stack = new Stack<(int index, SourceNode parent)>();
            stack.Push((0, this));
            while (stack.Count > 0)
            {
                var (index, parent) = stack.Pop();
                var node = parent.GetChild(index);
                yield return node;
                if (++index < parent.ChildrenCount)
                {
                    stack.Push((index, parent));
                }
                if (node.ChildrenCount > 0 && (descendIntoChildren?.Invoke(node) ?? true))
                {
                    stack.Push((0, node));
                }
            }
        }

        protected internal abstract IEnumerable<IContainer<SourceNode>> ChildrenLists();

        protected internal virtual int ChildrenCount => ChildrenLists().Sum(list => list.SlotCount);

        protected internal virtual SourceNode GetChild(int index)
        {
            foreach (var list in ChildrenLists())
            {
                index -= list.SlotCount;
                if (index < 0)
                {
                    return list.GetNodeSlot(index + list.SlotCount);
                }
            }
            throw new ArgumentOutOfRangeException(nameof(index));
        }
    }
}
