using System;
using System.Collections.Immutable;

namespace WarHub.ArmouryModel.Source
{
    [WhamNodeCore]
    public abstract partial record AbstractBaseCore
    {
        public string? BaseName { get; init; }

        public DateTime BaseDateTime { get; init; }

        public ImmutableArray<ItemCore> BaseItems { get; init; } = ImmutableArray<ItemCore>.Empty;
    }
}
