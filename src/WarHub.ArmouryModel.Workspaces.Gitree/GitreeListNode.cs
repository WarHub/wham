using System.Collections.Immutable;
using System.Diagnostics;

namespace WarHub.ArmouryModel.Workspaces.Gitree
{
    [DebuggerDisplay("{" + nameof(Name) + "}, Count = {" + nameof(Items) + ".Length}")]
    public record GitreeListNode(string Name)
    {
        public ImmutableArray<GitreeNode> Items { get; init; } = ImmutableArray<GitreeNode>.Empty;
    }
}
