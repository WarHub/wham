using System.Collections.Immutable;
using Amadevus.RecordGenerator;
using System.Diagnostics;

namespace WarHub.ArmouryModel.Workspaces.Gitree
{
    [Record]
    [DebuggerDisplay("{" + nameof(Name) + "}, Count = {" + nameof(Items) + ".Length}")]
    public partial class GitreeListNode
    {
        public string Name { get; }

        public ImmutableArray<GitreeNode> Items { get; }
    }
}
