using System;
using System.Collections.Immutable;

namespace WarHub.ArmouryModel.Source.CodeGeneration.Tests.GeneratedCode
{
    [WhamNodeCore]
    public abstract partial class AbstractBaseCore
    {
        public string BaseName { get; }

        public DateTime BaseDateTime { get; }

        public ImmutableArray<ItemCore> BaseItems { get; }
    }
}
