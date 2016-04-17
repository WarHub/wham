namespace WarHub.Armoury.Model.Builders
{
    using System.Collections.Generic;

    public interface IProfileBuilder : IApplicableVisibilityBuilder
    {
        ProfileLinkPair ProfileLinkPair { get; }
        string ApplicableName { get; set; }
        IReadOnlyCollection<ICharacteristicBuilder> CharacteristicBuilders { get; }
    }
}
