using System.Collections.Immutable;

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
