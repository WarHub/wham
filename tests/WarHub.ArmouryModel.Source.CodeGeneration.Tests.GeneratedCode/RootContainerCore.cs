using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;

namespace WarHub.ArmouryModel.Source.CodeGeneration.Tests.GeneratedCode
{
    [WhamNodeCore]
    public partial class RootContainerCore
    {
        public ImmutableArray<ContainerCore> LeftContainers { get; }

        public ImmutableArray<ContainerCore> RightContainers { get; }
    }
}
