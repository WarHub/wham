using System;
using System.Collections.Generic;

namespace WarHub.ArmouryModel.Source
{
    public readonly struct ChildInfo : IEquatable<ChildInfo>
    {
        public ChildInfo(string name, SourceNode node)
        {
            Name = name;
            Node = node;
        }

        public bool IsList => Node.IsList;

        public string Name { get; }

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

        public override int GetHashCode()
        {
            var hashCode = 1418292515;
            hashCode = (hashCode * -1521134295) + EqualityComparer<string>.Default.GetHashCode(Name);
            hashCode = (hashCode * -1521134295) + EqualityComparer<SourceNode>.Default.GetHashCode(Node);
            return hashCode;
        }

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
