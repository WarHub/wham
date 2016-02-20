// WarHub licenses this file to you under the MIT license.
// See LICENSE file in the project root for more information.

namespace WarHub.Armoury.Model
{
    using System;
    using System.Collections.Generic;

    public static class EnumerableExtensions
    {
        /// <summary>
        ///     Recursively enumerates all descendants in provided fashion (by concatenating each child and enumeration of
        ///     <see cref="AllDescendants{TNode,TChild}" /> invoked on it).
        /// </summary>
        /// <typeparam name="TNode">Super-type of child (possibly root).</typeparam>
        /// <typeparam name="TChild">Type of enumerated nodes.</typeparam>
        /// <param name="node">Node to have it's descendants enumerated recursively.</param>
        /// <param name="childrenSelector">Selects children.</param>
        /// <returns>All descendats enumerator.</returns>
        public static IEnumerable<TChild> AllDescendants<TNode, TChild>(this TNode node,
            Func<TNode, IEnumerable<TChild>> childrenSelector) where TChild : TNode
        {
            foreach (var child in childrenSelector(node))
            {
                yield return child;
                foreach (var descendant in child.AllDescendants(childrenSelector))
                {
                    yield return descendant;
                }
            }
        }

        /// <summary>
        ///     Appends original enumeration with additional <paramref name="item" /> yield returned after
        ///     <paramref name="enumerable" /> enumeration completes.
        /// </summary>
        /// <typeparam name="T">Type of collection items.</typeparam>
        /// <param name="enumerable">Original enumeration object.</param>
        /// <param name="item">Item appended to the end of enumeration.</param>
        /// <returns>Enumerable extended with the additional item at the end.</returns>
        public static IEnumerable<T> AppendWith<T>(this IEnumerable<T> enumerable, T item)
        {
            foreach (var originalItem in enumerable)
            {
                yield return originalItem;
            }
            yield return item;
        }

        /// <summary>
        ///     Prepends original enumeration with additional <paramref name="item" /> yield returned before
        ///     <paramref name="enumerable" /> enumeration begins.
        /// </summary>
        /// <typeparam name="T">Type of collection items.</typeparam>
        /// <param name="enumerable">Original enumeration object.</param>
        /// <param name="item">Item prepended to the enumeration.</param>
        /// <returns>Enumerable extended with the additional item at the beginning.</returns>
        public static IEnumerable<T> PrependWith<T>(this IEnumerable<T> enumerable, T item)
        {
            yield return item;
            foreach (var originalItem in enumerable)
            {
                yield return originalItem;
            }
        }
    }
}
