using System.Collections.Immutable;

namespace WarHub.ArmouryModel.Workspaces.Gitree
{
    [Record]
    public partial class JsonTreeItemList
    {
        public string Name { get; }

        public ImmutableArray<JsonTreeItem> Items { get; }
    }
}
