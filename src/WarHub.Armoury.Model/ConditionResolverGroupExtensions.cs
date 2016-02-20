// WarHub licenses this file to you under the MIT license.
// See LICENSE file in the project root for more information.

namespace WarHub.Armoury.Model
{
    using System;
    using System.Linq;

    public static class ConditionResolverGroupExtensions
    {
        public static bool IsMet(this IConditionResolver resolver, IGameSystemConditionGroup conditionGroup)
        {
            switch (conditionGroup.Type)
            {
                case ConditionGroupType.And:
                    return conditionGroup.Conditions.All(resolver.IsMet) &&
                           conditionGroup.ConditionGroups.All(resolver.IsMet);
                case ConditionGroupType.Or:
                    return conditionGroup.Conditions.Any(resolver.IsMet) ||
                           conditionGroup.ConditionGroups.Any(resolver.IsMet);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public static bool IsMet(this IConditionResolver resolver, ICatalogueConditionGroup conditionGroup)
        {
            switch (conditionGroup.Type)
            {
                case ConditionGroupType.And:
                    return conditionGroup.Conditions.All(resolver.IsMet) &&
                           conditionGroup.ConditionGroups.All(resolver.IsMet);
                case ConditionGroupType.Or:
                    return conditionGroup.Conditions.Any(resolver.IsMet) ||
                           conditionGroup.ConditionGroups.Any(resolver.IsMet);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
