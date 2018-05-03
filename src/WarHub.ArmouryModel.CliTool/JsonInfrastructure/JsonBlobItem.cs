using System.Collections.Immutable;
using WarHub.ArmouryModel.Source;

namespace WarHub.ArmouryModel.CliTool.JsonInfrastructure
{
    [Record]
    public partial class JsonBlobItem
    {
        public DatablobNode Node { get; }

        public SourceNode WrappedNode { get; }

        public bool IsLeaf { get; }

        public ImmutableArray<JsonBlobList> Children { get; }
    }
}
