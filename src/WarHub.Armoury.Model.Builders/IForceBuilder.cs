// WarHub licenses this file to you under the MIT license.
// See LICENSE file in the project root for more information.

namespace WarHub.Armoury.Model.Builders
{
    using System.Collections.Generic;

    public interface IForceBuilder : IApplicableGeneralLimitsBuilder, IForceBuilderNode
    {
        IEnumerable<ICategoryBuilder> CategoryBuilders { get; }

        IForce Force { get; }
    }
}
