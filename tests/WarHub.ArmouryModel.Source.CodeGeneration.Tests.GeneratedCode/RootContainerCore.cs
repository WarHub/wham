using System.Collections.Immutable;

namespace WarHub.ArmouryModel.Source.CodeGeneration.Tests.GeneratedCode
{
    [WhamNodeCore]
    public partial class RootContainerCore
    {
        public ImmutableArray<ContainerCore> LeftContainers { get; }

        public ImmutableArray<ContainerCore> RightContainers { get; }
    }
}
