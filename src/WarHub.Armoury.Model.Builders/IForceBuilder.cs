namespace WarHub.Armoury.Model.Builders
{
    using System.Collections.Generic;

    public interface IForceBuilder : IApplicableGeneralLimitsBuilder, IForceBuilderNode
    {
        IEnumerable<ICategoryBuilder> CategoryBuilders { get; }
        IForce Force { get; }
    }
}