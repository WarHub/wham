using System.Collections.Immutable;
using System.Diagnostics;
using WarHub.ArmouryModel.Source;

namespace WarHub.ArmouryModel.Workspaces.Gitree
{
    [Record]
    [DebuggerDisplay("{" + nameof(WrappedNode) + ".Kind}, Lists = {" + nameof(Lists) + ".Length}")]
    public partial class GitreeNode
    {
        public DatablobNode Datablob { get; }

        public SourceNode WrappedNode { get; }

        public bool IsLeaf { get; }

        public ImmutableArray<GitreeListNode> Lists { get; }

        public static GitreeNode Create(DatablobNode datablob, SourceNode node, ImmutableArray<GitreeListNode> lists)
        {
            return new GitreeNode(datablob, node, lists.IsEmpty, lists);
        }
    }
}
