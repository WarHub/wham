// WarHub licenses this file to you under the MIT license.
// See LICENSE file in the project root for more information.

namespace WarHub.Armoury.Model.BattleScribeXml
{
    using System;
    using System.Collections.Generic;

    public static class Utilities
    {
        public static IEnumerable<TResult> SelectWithNestedForces<TResult>(this IEnumerable<Force> forceList,
            Func<Force, TResult> selector)
        {
            if (forceList == null)
            {
                throw new ArgumentNullException(nameof(forceList));
            }
            if (selector == null)
            {
                throw new ArgumentNullException(nameof(selector));
            }
            return forceList.SelectWithNestedForcesInternal(selector);
        }

        private static IEnumerable<TResult> SelectWithNestedForcesInternal<TResult>(this IEnumerable<Force> forceList,
            Func<Force, TResult> selector)
        {
            foreach (var force in forceList)
            {
                yield return selector(force);
                foreach (var item in force.Forces.SelectWithNestedForces(selector))
                {
                    yield return item;
                }
            }
        }
    }
}
