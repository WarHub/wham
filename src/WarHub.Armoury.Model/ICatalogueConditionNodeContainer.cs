// WarHub licenses this file to you under the MIT license.
// See LICENSE file in the project root for more information.

namespace WarHub.Armoury.Model
{
    public interface ICatalogueConditionNodeContainer : ICatalogueItem
    {
        INodeSimple<ICatalogueConditionGroup> ConditionGroups { get; }

        INodeSimple<ICatalogueCondition> Conditions { get; }
    }
}
