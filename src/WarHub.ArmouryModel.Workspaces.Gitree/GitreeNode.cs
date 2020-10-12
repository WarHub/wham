using System.Collections.Immutable;
using System.Diagnostics;
using WarHub.ArmouryModel.Source;

namespace WarHub.ArmouryModel.Workspaces.Gitree
{
    [DebuggerDisplay("{" + nameof(WrappedNode) + ".Kind}, Lists = {" + nameof(Lists) + ".Length}")]
    public record GitreeNode(
        DatablobNode Datablob,
        SourceNode WrappedNode)
    {
        public bool IsLeaf { get; init; }

        public ImmutableArray<GitreeListNode> Lists { get; init; } = ImmutableArray<GitreeListNode>.Empty;

        public static GitreeNode Create(DatablobNode datablob, SourceNode node, ImmutableArray<GitreeListNode> lists)
        {
            return new GitreeNode(datablob, node)
            {
                IsLeaf = lists.IsEmpty,
                Lists = lists
            };
        }
    }
}
