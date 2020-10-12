using System;
using System.Collections.Generic;

namespace WarHub.ArmouryModel.Source
{
    /// <summary>
    /// A pair of a child node and a name of the child in parent.
    /// </summary>
    public readonly struct ChildInfo : IEquatable<ChildInfo>
    {
        /// <summary>
        /// Creates a new instance of <see cref="ChildInfo"/>.
        /// </summary>
        /// <param name="name">The name of the node.</param>
        /// <param name="node">The node with the given name.</param>
        public ChildInfo(string name, SourceNode node)
        {
            Name = name;
            Node = node;
        }

        /// <summary>
        /// True if <see cref="Node"/>'s <see cref="SourceNode.IsList"/> is <c>true</c>.
        /// </summary>
        public bool IsList => Node.IsList;

        /// <summary>
        /// Name this <see cref="Node"/> can be found under in its parent node.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// The node that is found under <see cref="Name"/> in its parent node.
        /// </summary>
        public SourceNode Node { get; }

        public override bool Equals(object? obj)
        {
            return obj is ChildInfo info && Equals(info);
        }

        public bool Equals(ChildInfo other)
        {
            return Name == other.Name &&
                   EqualityComparer<SourceNode>.Default.Equals(Node, other.Node);
        }

#if NETSTANDARD2_0
        public override int GetHashCode()
        {
            var hashCode = 1418292515;
            hashCode = (hashCode * -1521134295) + EqualityComparer<string>.Default.GetHashCode(Name);
            hashCode = (hashCode * -1521134295) + EqualityComparer<SourceNode>.Default.GetHashCode(Node);
            return hashCode;
        }
#else
        public override int GetHashCode() => HashCode.Combine(Name, Node);
#endif

        public static bool operator ==(ChildInfo left, ChildInfo right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(ChildInfo left, ChildInfo right)
        {
            return !(left == right);
        }
    }
}
