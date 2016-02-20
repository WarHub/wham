// WarHub licenses this file to you under the MIT license.
// See LICENSE file in the project root for more information.

namespace WarHub.Armoury.Model
{
    public interface IGameSystemConditionGroup : IConditionGroup, IGameSystemItem
    {
        INodeSimple<IGameSystemConditionGroup> ConditionGroups { get; }

        INodeSimple<IGameSystemCondition> Conditions { get; }
    }
}
