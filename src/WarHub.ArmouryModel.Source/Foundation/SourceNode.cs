using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace WarHub.ArmouryModel.Source
{
    /// <summary>
    /// Base class of all Source nodes, providing an interface and base implementation of many of it's methods.
    /// This class is abstract.
    /// </summary>
    public abstract partial class SourceNode : INodeWithCore<NodeCore>
    {
        public SourceNode(NodeCore core, SourceNode parent)
        {
            Core = core;
            Parent = parent;
            Tree = parent?.Tree;
        }

        private int? _indexInParent;

        NodeCore INodeWithCore<NodeCore>.Core => Core;

        protected SourceNode Parent { get; }

        internal SourceTree Tree { get; set; }

        internal NodeCore Core { get; }

        /// <summary>
        /// Gets the kind of this node.
        /// </summary>
        public abstract SourceKind Kind { get; }

        /// <summary>
        /// Gets index in parent, or -1 if no parent.
        /// </summary>
        public int IndexInParent => _indexInParent ?? CalculateAndSaveIndexInParent();

        /// <summary>
        /// Traverses ancestry path and returns each node beginning with this node's parent, if any.
        /// May yield no results if this node is a root node.
        /// </summary>
        /// <returns>Enumeration of this node's ancestors.</returns>
        public IEnumerable<SourceNode> Ancestors()
        {
            return AncestorsCore(includeSelf: false);
        }

        /// <summary>
        /// Traverses ancestry path and returns each node beginning with this node.
        /// This will always yield at least this node.
        /// </summary>
        /// <returns>Enumeration of this node and its ancestors.</returns>
        public IEnumerable<SourceNode> AncestorsAndSelf()
        {
            return AncestorsCore(includeSelf: true);
        }

        /// <summary>
        /// Enumerates all children of this node.
        /// </summary>
        /// <returns>Enumeration of this node's children.</returns>
        public IEnumerable<SourceNode> Children()
        {
            for (int i = 0; i < ChildrenCount; i++)
            {
                yield return GetChild(i);
            }
        }

        /// <summary>
        /// Determines if the <paramref name="node"/> is any of this node's descendants.
        /// </summary>
        /// <param name="node">Node to be checked of being a descendant.</param>
        /// <returns>True if this node is an ancestor of given <paramref name="node"/>.</returns>
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

        /// <summary>
        /// Traverses all descendants of this node depth-first. <paramref name="descendIntoChildren"/>'s default
        /// value null is the same as if it always returned null (x => true).
        /// </summary>
        /// <param name="descendIntoChildren">Predicate to decide if node's children should be visited.</param>
        /// <returns>Enumeration of traversal.</returns>
        public IEnumerable<SourceNode> Descendants(Func<SourceNode, bool> descendIntoChildren = null)
        {
            return DescendantsCore(descendIntoChildren, includeSelf: false);
        }

        /// <summary>
        /// Traverses all descendants of this node depth-first. <paramref name="descendIntoChildren"/>'s default
        /// value null is the same as if it always returned null (x => true). At the beginning this node is returned.
        /// </summary>
        /// <param name="descendIntoChildren">Predicate to decide if node's children should be visited.</param>
        /// <returns>Enumeration of traversal.</returns>
        public IEnumerable<SourceNode> DescendantsAndSelf(Func<SourceNode, bool> descendIntoChildren = null)
        {
            return DescendantsCore(descendIntoChildren, includeSelf: true);
        }

        /// <summary>
        /// Traverses ancestry path up to the root and finds first node that is assignable to
        /// <typeparamref name="TNode"/> type and satisfying provided <paramref name="predicate"/>.
        /// First visited node is this node.
        /// </summary>
        /// <typeparam name="TNode">Type of node to return.</typeparam>
        /// <param name="predicate">Determines if the node should be returned.</param>
        /// <returns>First ancestor (or self) that satisfies both conditions.</returns>
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

        public virtual IEnumerable<NamedNodeOrList> NamedChildrenLists()
        {
            return Enumerable.Empty<NamedNodeOrList>();
        }

        /// <summary>
        /// Enumerates containers of children in this node. Implementation is used
        /// to provide the very basic and totally inefficient implementation of
        /// <see cref="ChildrenCount"/> and <see cref="GetChild(int)"/>. It is advised
        /// to provide a specialized and optimized implementation of these members.
        /// </summary>
        /// <returns></returns>
        protected internal virtual IEnumerable<NodeOrList> ChildrenLists()
        {
            return Enumerable.Empty<NodeOrList>();
        }

        /// <summary>
        /// Gets the total count of children of this node. <see cref="SourceNode"/>'s basic
        /// implementation is very inefficient. It is advised that deriving classes provide
        /// a specialized and optimized implementation of this member.
        /// </summary>
        protected internal virtual int ChildrenCount => ChildrenLists().Sum(list => list.Count);

        /// <summary>
        /// Retrieves this node's child from given <paramref name="index"/>. <see cref="SourceNode"/>'s basic
        /// implementation is very inefficient. It is advised that deriving classes provide
        /// a specialized and optimized implementation of this member.
        /// </summary>
        /// <param name="index">The index from which to retrieve child</param>
        /// <returns>Child from the given index.</returns>
        /// <exception cref="ArgumentOutOfRangeException">When index is out of range.</exception>
        protected internal virtual SourceNode GetChild(int index)
        {
            foreach (var list in ChildrenLists())
            {
                index -= list.Count;
                if (index < 0)
                {
                    return list[index + list.Count];
                }
            }
            throw new ArgumentOutOfRangeException(nameof(index));
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

        private int CalculateAndSaveIndexInParent()
        {
            var indexInParent = CalculateIndex();
            _indexInParent = indexInParent;
            return indexInParent;
            int CalculateIndex()
            {
                if (Parent == null)
                {
                    return -1;
                }
                int index = 0;
                foreach (var sibling in Parent.Children())
                {
                    if (ReferenceEquals(this, sibling))
                    {
                        break;
                    }
                    index++;
                }
                return index;
            }
        }

        public static implicit operator NodeOrList(SourceNode node) => new NodeOrList(node);

        public struct NodeOrList
        {
            public NodeOrList(SourceNode singleChild)
            {
                SingleChild = singleChild;
                List = default;
            }
            public NodeOrList(NodeList<SourceNode> list)
            {
                SingleChild = default;
                List = list;
            }

            public bool IsSingle => SingleChild != null;
            public bool IsList => SingleChild == null;
            public int Count => IsSingle ? 1 : List.Count;
            public SourceNode this[int index]
            {
                get
                {
                    return
                        IsList ? List[index] :
                        index == 0 ? SingleChild :
                        throw new IndexOutOfRangeException(
                            "This is a single element union, tried to access index " + index);
                }
            }

            public SourceNode SingleChild { get; }
            public NodeList<SourceNode> List { get; }
        }
    }
}
