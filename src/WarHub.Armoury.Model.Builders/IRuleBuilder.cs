namespace WarHub.Armoury.Model.Builders
{
    public interface IRuleBuilder : IApplicableVisibilityBuilder
    {
        RuleLinkPair RuleLinkPair { get; }

        string ApplicableName { get; set; }
        string ApplicableDescription { get; set; }
    }
}
