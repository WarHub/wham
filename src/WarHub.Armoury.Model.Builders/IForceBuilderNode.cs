namespace WarHub.Armoury.Model.Builders
{
    using System.Collections.Generic;

    public interface IForceBuilderNode
    {
        IEnumerable<IForceBuilder> ForceBuilders { get; }
    }
}