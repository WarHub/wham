using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace WarHub.ArmouryModel.Source
{
    [DebuggerDisplay("Count = {" + nameof(Count) + "}")]
    public readonly partial struct NodeList<TNode> : IReadOnlyList<TNode>, IContainerProvider<TNode>, IEquatable<NodeList<TNode>>
        where TNode : SourceNode
    {
        // TODO a lot of optimizations here

        internal NodeList(IContainer<TNode>? container)
        {
            Container = container;
        }

        internal IContainer<TNode>? Container { get; }

        // We accept NullReferenceException here for perf. We'd get IndexOutOfRangeException instead.
        public TNode this[int index] => Container!.GetNodeSlot(index);

        public int Count => Container?.SlotCount ?? 0;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        IContainer<TNode>? IContainerProvider<TNode>.Container => Container;

        public Enumerator GetEnumerator() => new Enumerator(this);

        IEnumerator<TNode> IEnumerable<TNode>.GetEnumerator() => EnumeratorObject.Create(this);

        IEnumerator IEnumerable.GetEnumerator() => EnumeratorObject.Create(this);

        public NodeList<TNode> Add(TNode item)
        {
            return this.Append(item).ToNodeList();
        }

        public NodeList<TNode> AddRange(IEnumerable<TNode> items)
        {
            return this.Concat(items).ToNodeList();
        }

        public NodeList<TNode> Slice(int start, int count)
        {
            var self = this;
            return (start, count) switch
            {
                (var st, _) when st < 0 && st >= self.Count =>
                    throw new ArgumentOutOfRangeException(nameof(start)),
                (_, var ct) when ct < 0 || ct > self.Count =>
                    throw new ArgumentOutOfRangeException(nameof(count)),

                (_, 0) => default,
                (0, var ct) when ct == self.Count => self,

                _ => (self.Container switch
                {
                    SliceContainerView s => new SliceContainerView(s.Container, s.Start + start, count),
                    { } c => new SliceContainerView(c, start, count),
                    null => null
                }).ToNodeList()
            };
        }

        public NodeList<SourceNode> ToNodeList()
        {
            return new NodeList<SourceNode>(Container);
        }

        // equality implementation
        public bool Equals(NodeList<TNode> other)
        {
            return Equals(Container, other.Container);
        }

        public override bool Equals(object? obj)
        {
            if (obj is null) return false;
            return obj is NodeList<TNode> list && Equals(list);
        }

        public override int GetHashCode()
        {
            return Container?.GetHashCode() ?? 0;
        }

        public static bool operator ==(NodeList<TNode> left, NodeList<TNode> right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(NodeList<TNode> left, NodeList<TNode> right)
        {
            return !(left == right);
        }

        public static implicit operator NodeList<SourceNode>(NodeList<TNode> nodeList)
        {
            return new NodeList<SourceNode>(nodeList.Container);
        }

        private sealed class SliceContainerView : IContainer<TNode>
        {
            public SliceContainerView(IContainer<TNode> container, int start, int count)
            {
                Container = container;
                Start = start;
                SlotCount = count;
            }

            public IContainer<TNode> Container { get; }

            public int SlotCount { get; }

            public int Start { get; }

            public TNode GetNodeSlot(int index) => Container.GetNodeSlot(Start + index);
        }
    }
}
