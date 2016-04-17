// WarHub licenses this file to you under the MIT license.
// See LICENSE file in the project root for more information.

namespace WarHub.Armoury.Model.Builders
{
    public interface IRuleBuilder : IApplicableVisibilityBuilder
    {
        string ApplicableDescription { get; set; }

        string ApplicableName { get; set; }

        RuleLinkPair RuleLinkPair { get; }
    }
}
