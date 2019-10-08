using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace WarHub.ArmouryModel.Source
{
    /// <summary>
    /// Base class of all Source nodes, providing an interface and base implementation of many of it's methods.
    /// This class is abstract.
    /// </summary>
    [DebuggerDisplay("{" + nameof(Kind) + "}, Children = {" + nameof(ChildrenCount) + "}")]
    public abstract partial class SourceNode : INodeWithCore<NodeCore>
    {
        public SourceNode(NodeCore core, SourceNode parent)
        {
            Core = core;
            Parent = parent;
            Tree = parent?.Tree;
        }

        internal int? _indexInParent;

        NodeCore INodeWithCore<NodeCore>.Core => Core;

        public SourceNode Parent { get; }

        internal SourceTree Tree { get; set; }

        protected internal NodeCore Core { get; }

        /// <summary>
        /// Gets the kind of this node.
        /// </summary>
        public abstract SourceKind Kind { get; }

        /// <summary>
        /// Gets whether or not this is a <see cref="ListNode{TChild}"/>.
        /// </summary>
        public virtual bool IsList => false;

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
        public virtual IEnumerable<SourceNode> Children()
        {
            return Enumerable.Empty<SourceNode>();
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
        /// value null is the same as if it always returned true (x => true).
        /// </summary>
        /// <param name="descendIntoChildren">Predicate to decide if node's children should be visited.</param>
        /// <returns>Enumeration of traversal.</returns>
        public IEnumerable<SourceNode> Descendants(Func<SourceNode, bool> descendIntoChildren = null)
        {
            return DescendantsCore(descendIntoChildren, includeSelf: false);
        }

        /// <summary>
        /// Traverses all descendants of this node depth-first. <paramref name="descendIntoChildren"/>'s default
        /// value null is the same as if it always returned true (x => true). At the beginning this node is returned.
        /// </summary>
        /// <param name="descendIntoChildren">Predicate to decide if node's children should be visited.</param>
        /// <returns>Enumeration of traversal.</returns>
        public IEnumerable<SourceNode> DescendantsAndSelf(Func<SourceNode, bool> descendIntoChildren = null)
        {
            return DescendantsCore(descendIntoChildren, includeSelf: true);
        }

        /// <summary>
        /// Traverses ancestry path up to the root and finds first node that is assignable to
        /// <typeparamref name="TNode"/> type (and satisfying <paramref name="predicate"/> if provided).
        /// First visited node is this node.
        /// </summary>
        /// <typeparam name="TNode">Type of node to return.</typeparam>
        /// <param name="predicate">Determines if the node should be returned. If null, predicate check is skipped.</param>
        /// <returns>First ancestor (or self) that satisfies both conditions.</returns>
        public TNode FirstAncestorOrSelf<TNode>(Func<TNode, bool> predicate = null) where TNode : class
        {
            var node = this;
            if (predicate is null)
            {
                while (node != null)
                {
                    if (node is TNode typedNode)
                    {
                        return typedNode;
                    }
                    node = node.Parent;
                }
            }
            else
            {
                while (node != null)
                {
                    if (node is TNode typedNode && predicate(typedNode))
                    {
                        return typedNode;
                    }
                    node = node.Parent;
                }
            }
            return null;
        }

        /// <summary>
        /// Enumerates children wrapped in <see cref="ChildInfo"/> which also provides property name
        /// by which a child node can be accessed from this instance.
        /// </summary>
        /// <returns>Children info wrapper enumeration.</returns>
        public virtual IEnumerable<ChildInfo> ChildrenInfos()
        {
            return Enumerable.Empty<ChildInfo>();
        }

        /// <summary>
        /// Gets the total count of children of this node.
        /// </summary>
        /// <remarks>
        /// <see cref="SourceNode"/>'s basic implementation is very inefficient. It is advised
        /// that deriving classes provide a specialized and optimized implementation of this member.
        /// </remarks>
        protected internal virtual int ChildrenCount => Children().Count();

        /// <summary>
        /// Retrieves this node's child from given <paramref name="index"/>.
        /// </summary>
        /// <remarks>
        /// <see cref="SourceNode"/>'s basic implementation is very inefficient. It is advised
        /// that deriving classes provide a specialized and optimized implementation of this member.
        /// </remarks>
        /// <param name="index">The index from which to retrieve child</param>
        /// <returns>Child from the given index.</returns>
        /// <exception cref="ArgumentOutOfRangeException">When index is out of range.</exception>
        protected internal virtual SourceNode GetChild(int index)
        {
            if (index < 0 || index >= ChildrenCount)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }
            return Children().ElementAt(index);
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
    }
}
