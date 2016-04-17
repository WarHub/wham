// WarHub licenses this file to you under the MIT license.
// See LICENSE file in the project root for more information.

namespace WarHub.Armoury.Model.Builders
{
    public interface IApplicableGeneralLimitsBuilder : IBuilderCore
    {
        ILimits<int, decimal, int> ApplicableGeneralLimits { get; }
    }
}
