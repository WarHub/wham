using System.Collections.Immutable;

namespace WarHub.ArmouryModel.Source
{
    [WhamNodeCore]
    public partial class RootContainerCore
    {
        public ImmutableArray<ContainerCore> LeftContainers { get; }

        public ImmutableArray<ContainerCore> RightContainers { get; }
    }
}
