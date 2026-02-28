using System;
using System.Collections.Generic;
using System.Linq;

namespace WarHub.ArmouryModel.Source.CodeGeneration
{
    internal static class EnumerableExtensions
    {
        public static IEnumerable<KeyValuePair<int, TSource>> Index<TSource>(this IEnumerable<TSource> source)
        {
            var index = 0;
            foreach (var item in source)
            {
                yield return new KeyValuePair<int, TSource>(index++, item);
            }
        }

        public static TSource AggregateRight<TSource>(this IEnumerable<TSource> source, Func<TSource, TSource, TSource> func)
        {
            var list = source.ToList();
            if (list.Count == 0)
                throw new InvalidOperationException("Sequence contains no elements.");
            var result = list[list.Count - 1];
            for (var i = list.Count - 2; i >= 0; i--)
            {
                result = func(list[i], result);
            }
            return result;
        }

        public static TAccumulate AggregateRight<TSource, TAccumulate>(
            this IEnumerable<TSource> source,
            TAccumulate seed,
            Func<TSource, TAccumulate, TAccumulate> func)
        {
            var list = source.ToList();
            var result = seed;
            for (var i = list.Count - 1; i >= 0; i--)
            {
                result = func(list[i], result);
            }
            return result;
        }

        public static IEnumerable<TResult> TagFirstLast<TSource, TResult>(
            this IEnumerable<TSource> source,
            Func<TSource, bool, bool, TResult> resultSelector)
        {
            using var enumerator = source.GetEnumerator();
            if (!enumerator.MoveNext())
                yield break;
            var current = enumerator.Current;
            var isFirst = true;
            while (enumerator.MoveNext())
            {
                yield return resultSelector(current, isFirst, false);
                current = enumerator.Current;
                isFirst = false;
            }
            yield return resultSelector(current, isFirst, true);
        }
        public static (IEnumerable<TSource> True, IEnumerable<TSource> False) Partition<TSource>(
            this IEnumerable<TSource> source,
            Func<TSource, bool> predicate)
        {
            var trueList = new List<TSource>();
            var falseList = new List<TSource>();
            foreach (var item in source)
            {
                if (predicate(item))
                    trueList.Add(item);
                else
                    falseList.Add(item);
            }
            return (trueList, falseList);
        }
    }
}
