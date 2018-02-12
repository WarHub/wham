using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;

namespace WarHub.ArmouryModel.Source.CodeGeneration.Tests.GeneratedCode
{
    [WhamNodeCore]
    public partial class RecursiveContainerCore
    {
        public string Name { get; }

        public ImmutableArray<ItemCore> Items { get; }

        public ImmutableArray<RecursiveContainerCore> Containers { get; }
    }
}
