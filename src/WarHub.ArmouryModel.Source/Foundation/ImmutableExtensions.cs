using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace WarHub.ArmouryModel.Source
{
    public static class ImmutableExtensions
    {
        public static ImmutableArray<TCore> ToImmutableRecursive<TBuilder, TCore>(this ImmutableArray<TBuilder>.Builder builders)
            where TBuilder : IBuilder<TCore>
        {
            var count = builders.Count;
            var resultBuilder = ImmutableArray.CreateBuilder<TCore>(count);
            for (int i = 0; i < count; i++)
            {
                resultBuilder.Add(builders[i].ToImmutable());
            }
            return resultBuilder.MoveToImmutable();
        }

        public static ImmutableArray<TCore> ToImmutableRecursive<TBuilder, TCore>(this List<TBuilder> builders)
            where TBuilder : IBuilder<TCore>
        {
            var count = builders.Count;
            var resultBuilder = ImmutableArray.CreateBuilder<TCore>(count);
            for (int i = 0; i < count; i++)
            {
                resultBuilder.Add(builders[i].ToImmutable());
            }
            return resultBuilder.MoveToImmutable();
        }

        public static List<TBuilder> ToBuildersList<TCore, TBuilder>(this ImmutableArray<TCore> cores)
            where TCore : IBuildable<TCore, TBuilder>
            where TBuilder : IBuilder<TCore>
        {
            return cores.Select(x => x.ToBuilder()).ToList();
        }

        public static void AddRangeAsBuilders<TCore, TBuilder>(this ImmutableArray<TBuilder>.Builder builders, ImmutableArray<TCore> cores)
            where TCore : IBuildable<TCore, TBuilder>
            where TBuilder : IBuilder<TCore>
        {
            builders.AddRange(cores.Select(x => x.ToBuilder()));
        }

        public static void AddRangeAsBuilders<TCore, TBuilder>(this List<TBuilder> builders, ImmutableArray<TCore> cores)
            where TCore : IBuildable<TCore, TBuilder>
            where TBuilder : IBuilder<TCore>
        {
            builders.AddRange(cores.Select(x => x.ToBuilder()).ToCollectionLikeWrapper(cores.Length));
        }

        private static CollectionLikeWrapper<T> ToCollectionLikeWrapper<T>(this IEnumerable<T> collection, int declaredCount)
        {
            return new CollectionLikeWrapper<T>(collection, declaredCount);
        }

        private struct CollectionLikeWrapper<T> : ICollection<T>
        {
            public CollectionLikeWrapper(IEnumerable<T> collection, int declaredCount)
            {
                _collection = collection;
                Count = declaredCount;
            }

            private IEnumerable<T> _collection;

            public int Count { get; }

            public bool IsReadOnly => true;

            public void Add(T item) => throw new NotSupportedException();

            public void Clear() => throw new NotSupportedException();

            public bool Contains(T item) => throw new NotSupportedException();

            public void CopyTo(T[] array, int arrayIndex)
            {
                foreach (var item in _collection)
                {
                    array[arrayIndex++] = item;
                }
            }

            public IEnumerator<T> GetEnumerator() => _collection.GetEnumerator();

            public bool Remove(T item) => throw new NotSupportedException();

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }
    }
}