// WarHub licenses this file to you under the MIT license.
// See LICENSE file in the project root for more information.

namespace WarHub.Armoury.Model.BattleScribe
{
    using System.Collections.Generic;

    public static class ContextExtensions
    {
        public static void ChangeContext<T>(this IEnumerable<T> collection,
            ICatalogueContext context)
            where T : ICatalogueItem
        {
            foreach (var item in collection)
            {
                item.Context = context;
            }
        }

        public static void ChangeContext<T>(this IEnumerable<T> collection,
            IGameSystemContext context)
            where T : IGameSystemItem
        {
            foreach (var item in collection)
            {
                item.Context = context;
            }
        }

        public static void ChangeContext<T>(this IEnumerable<T> collection,
            IRosterContext context)
            where T : IRosterItem
        {
            foreach (var item in collection)
            {
                item.Context = context;
            }
        }

        public static void ChangeContext<T>(this IEnumerable<T> collection,
            IForceContext context)
            where T : IForceItem
        {
            foreach (var item in collection)
            {
                item.ForceContext = context;
            }
        }
    }
}
