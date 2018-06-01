using System.Collections.Immutable;
using WarHub.ArmouryModel.Source;

namespace WarHub.ArmouryModel.Workspaces.Gitree
{
    [Record]
    public partial class JsonTreeItem
    {
        public DatablobNode Node { get; }

        public SourceNode WrappedNode { get; }

        public bool IsLeaf { get; }

        public ImmutableArray<JsonTreeItemList> Lists { get; }
    }
}
