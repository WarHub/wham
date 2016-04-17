namespace WarHub.Armoury.Model.Builders
{
    public interface IGroupBuilder : IApplicableVisibilityBuilder, IApplicableEntryLimitsBuilder, IEntryBuilderNode
    {
        GroupLinkPair GroupLinkPair { get; }
    }
}
