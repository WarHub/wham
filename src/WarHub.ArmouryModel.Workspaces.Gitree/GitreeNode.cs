using System.Collections.Immutable;
using WarHub.ArmouryModel.Source;

namespace WarHub.ArmouryModel.Workspaces.Gitree
{
    [Record]
    public partial class GitreeNode
    {
        public DatablobNode Node { get; }

        public SourceNode WrappedNode { get; }

        public bool IsLeaf { get; }

        public ImmutableArray<GitreeListNode> Lists { get; }

        public static GitreeNode CreateNonLeaf(DatablobNode datablob, SourceNode node, ImmutableArray<GitreeListNode> lists)
        {
            return new GitreeNode(datablob, node, false, lists);
        }

        public static GitreeNode CreateLeaf(DatablobNode datablob, SourceNode node)
        {
            return new GitreeNode(datablob, node, true, ImmutableArray<GitreeListNode>.Empty);
        }
    }
}
