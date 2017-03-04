// WarHub licenses this file to you under the MIT license.
// See LICENSE file in the project root for more information.

namespace WarHub.Armoury.Model.Source
{
    using System.Collections.Immutable;

    public abstract class SourceTree
    {
        public abstract SourceNode GetRootNode();
    }

    public abstract class SourceNode
    {
        protected SourceNode(SourceTree tree)
        {
            Tree = tree;
        }

        protected SourceTree Tree { get; }

        public abstract SourceNode Parent { get; }

        public abstract SourceNodeKind NodeKind { get; }

        public abstract SourceNodeList Children { get; }
    }

    public abstract class ImmutableSourceNode
    {
        public abstract ImmutableArray<ImmutableSourceNode> Children { get; }

        public abstract SourceNodeKind NodeKind { get; }
    }

    public struct SourceNodeList
    {
    }

    /// <summary>
    /// Represents the kind of <see cref="SourceNode"/>. Wrapper class for string.
    /// </summary>
    public struct SourceNodeKind
    {
        public SourceNodeKind(string stringValue)
        {
            StringValue = stringValue;
        }

        public static readonly SourceNodeKind Unspecified = default(SourceNodeKind);

        public string StringValue { get; }
    }
}
