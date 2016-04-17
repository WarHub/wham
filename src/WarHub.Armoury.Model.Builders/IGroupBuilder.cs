// WarHub licenses this file to you under the MIT license.
// See LICENSE file in the project root for more information.

namespace WarHub.Armoury.Model.Builders
{
    public interface IGroupBuilder : IApplicableVisibilityBuilder, IApplicableEntryLimitsBuilder, IEntryBuilderNode
    {
        GroupLinkPair GroupLinkPair { get; }
    }
}
