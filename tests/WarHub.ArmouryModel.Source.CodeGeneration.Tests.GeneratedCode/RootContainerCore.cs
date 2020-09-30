using System.Collections.Immutable;

namespace WarHub.ArmouryModel.Source
{
    [WhamNodeCore]
    public partial record RootContainerCore
    {
        public ImmutableArray<ContainerCore> LeftContainers { get; init; } = ImmutableArray<ContainerCore>.Empty;

        public ImmutableArray<ContainerCore> RightContainers { get; init; } = ImmutableArray<ContainerCore>.Empty;
    }
}
