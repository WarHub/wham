namespace WarHub.Armoury.Model.Builders
{
    public interface IApplicableGeneralLimitsBuilder : IBuilderCore
    {
        ILimits<int, decimal, int> ApplicableGeneralLimits { get; }
    }
}
