// WarHub licenses this file to you under the MIT license.
// See LICENSE file in the project root for more information.

namespace WarHub.Armoury.Model.Builders
{
    using System.Collections.Generic;

    public interface IProfileBuilder : IApplicableVisibilityBuilder
    {
        string ApplicableName { get; set; }

        IReadOnlyCollection<ICharacteristicBuilder> CharacteristicBuilders { get; }

        ProfileLinkPair ProfileLinkPair { get; }
    }
}
