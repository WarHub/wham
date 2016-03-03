// WarHub licenses this file to you under the MIT license.
// See LICENSE file in the project root for more information.

namespace WarHub.Armoury.Model.BattleScribeTests
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using Xunit;

    /// <summary>
    ///     Test checking how ObservableCollection works, and it's underlying list.
    ///     Also trying to solve problem with readonly collection
    /// </summary>
    public class ObservableCollectionTests
    {
        [Fact]
        public void MoveItemBehaviorTest()
        {
            var objectArray = new[] {new object(), new object(), new object(), new object(), new object()};
            var collection = new ObservableCollection<object>(objectArray);
            collection.Move(2, 4);
            Assert.Same(objectArray[2], collection[4]);
            Assert.Same(objectArray[3], collection[2]);
        }

        [Fact]
        public void ObsCollAddToInternalListTest()
        {
            var observable = new IntCollection();
            var counter = 0;
            observable.CollectionChanged += (sender, args) => counter++;
            var list = observable.ItemList;
            observable.Add(1);
            Assert.Throws<NotSupportedException>(() => list.Add(2));
            Assert.Equal(1, observable.Count);
            // observable collection uses the same list as the one returned by Items
            Assert.Equal(list.Count, observable.Count); // list and collection is the same underlying entity
            Assert.Equal(1, counter); // adding to underlying list doesn't call notifications
        }

        [Fact]
        public void ObsCollCloningCtorListTest()
        {
            var intList = new List<int>();
            intList.Add(0);
            intList.Add(1);
            intList.Add(2);
            var intCollection = new IntCollection(intList);
            intList.Add(4);
            // proof that a creating observable collection clones the list from constructor.
            Assert.NotEqual(intList.Count, intCollection.UnderlyingList.Count);
            Assert.NotEqual(intList, intCollection.UnderlyingList);
        }

        internal class IntCollection : ObservableCollection<int>
        {
            public IntCollection()
            {
            }

            public IntCollection(IEnumerable<int> list)
                : base(list)
            {
            }

            public IList<int> ItemList => new ReadOnlyCollection<int>(Items);

            public IList<int> UnderlyingList => Items;
        }
    }
}
