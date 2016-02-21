// WarHub licenses this file to you under the MIT license.
// See LICENSE file in the project root for more information.

namespace WarHub.Armoury.Model.BattleScribeXml
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public static class Utilities
    {
        /// <summary>
        ///     For each element on the list action is called with argument being this element.
        /// </summary>
        /// <typeparam name="T">Type of objects in list.</typeparam>
        /// <param name="sourceList">List to take elements from.</param>
        /// <param name="action">Action to be called with each element.</param>
        public static void ForEach<T>(this IEnumerable<T> sourceList, Action<T> action)
        {
            foreach (var item in sourceList)
            {
                action(item);
            }
        }

        /// <summary>
        ///     Creates a new list containing items produced by transforming items from source.
        /// </summary>
        /// <typeparam name="T">Type of objects in source list.</typeparam>
        /// <typeparam name="TResult">Type of objects in result list.</typeparam>
        /// <param name="sourceList">List containing objects to transform.</param>
        /// <param name="transformation">Function transforming objects from one type to the other.</param>
        /// <returns></returns>
        public static List<TResult> TransCreate<T, TResult>(this List<T> sourceList, Func<T, TResult> transformation)
        {
            return new List<TResult>(sourceList.Select(transformation));
        }
    }
}
