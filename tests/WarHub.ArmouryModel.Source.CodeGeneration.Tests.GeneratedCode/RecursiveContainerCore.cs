using System.Collections.Immutable;

namespace WarHub.ArmouryModel.Source
{
    [WhamNodeCore]
    public partial record RecursiveContainerCore
    {
        public string? Name { get; init; }

        public ImmutableArray<ItemCore> Items { get; init; } = ImmutableArray<ItemCore>.Empty;

        public ImmutableArray<RecursiveContainerCore> Containers { get; init; } = ImmutableArray<RecursiveContainerCore>.Empty;
    }
}
