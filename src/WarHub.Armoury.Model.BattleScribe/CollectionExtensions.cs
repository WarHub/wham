namespace WarHub.Armoury.Model.BattleScribe
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using BattleScribeXml.GuidMapping;

    public static class CollectionExtensions
    {
        public static void AddRange<T>(this ICollection<T> collection, IEnumerable<T> range)
        {
            foreach (var item in range)
            {
                collection.Add(item);
            }
        }

        public static List<T> FindPathTo<T>(this IEnumerable<T> collection, Predicate<T> targetChecker,
            Func<T, IEnumerable<T>> nodeAccessor)
        {
            return collection.FindPath(targetChecker, nodeAccessor, new List<T>());
        }

        public static string CombineLinkedId(this List<string> guids)
        {
            return GuidController.CombineLinkedId(guids);
        }

        private static List<T> FindPath<T>(this IEnumerable<T> collection, Predicate<T> targetChecker,
            Func<T, IEnumerable<T>> nodeAccessor, List<T> currentPath)
        {
            foreach (var item in collection)
            {
                var path = currentPath.ToList();
                path.Add(item);
                if (targetChecker(item))
                {
                    return path;
                }
                path = nodeAccessor(item).FindPath(targetChecker, nodeAccessor, path);
                if (path != null)
                {
                    return path;
                }
            }
            return null;
        }
    }
}
