using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;
using WarHub.ArmouryModel.Source;

namespace WarHub.ArmouryModel.CliTool.JsonUtilities
{
    [Record]
    public partial class NodeFolder
    {
        public DatablobNode Node { get; }

        public SourceKind NodeKind { get; }

        public ImmutableArray<ListFolder> Children { get; }
    }

    [Record]
    public partial class ListFolder
    {
        public string Name { get; }

        public ImmutableArray<NodeFolder> Nodes { get; }
    }
}
