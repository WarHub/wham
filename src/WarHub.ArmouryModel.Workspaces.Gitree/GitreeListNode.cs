using System.Collections.Immutable;

namespace WarHub.ArmouryModel.Workspaces.Gitree
{
    [Record]
    public partial class GitreeListNode
    {
        public string Name { get; }

        public ImmutableArray<GitreeNode> Items { get; }
    }
}
