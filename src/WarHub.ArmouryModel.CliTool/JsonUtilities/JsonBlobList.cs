using System.Collections.Immutable;

namespace WarHub.ArmouryModel.CliTool.JsonUtilities
{
    [Record]
    public partial class JsonBlobList
    {
        public string Name { get; }

        public ImmutableArray<JsonBlobItem> Nodes { get; }
    }
}
