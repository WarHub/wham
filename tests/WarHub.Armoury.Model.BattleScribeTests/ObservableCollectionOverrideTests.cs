// WarHub licenses this file to you under the MIT license.
// See LICENSE file in the project root for more information.

namespace WarHub.Armoury.Model.BattleScribeTests
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using Xunit;

    public class ObservableCollectionOverrideTests
    {
        [Fact]
        public void AddAndRemoveTest()
        {
            var x = new Derivate {0};
            x.Remove(0);
            // assert single insert and single remove call
            x.AssertCounterValues(0, 1, 0, 1, 0);
        }

        [Fact]
        public void AddTest()
        {
            var x = new Derivate {0};
            // assert single insert call
            x.AssertCounterValues(0, 1, 0, 0, 0);
        }

        [Fact]
        public void ClearTest()
        {
            var x = new Derivate();
            x.Clear();
            // assert single clear call
            x.AssertCounterValues(1, 0, 0, 0, 0);
        }

        [Fact]
        public void InitialListNotUsingInsertItemTest()
        {
            const int items = 5;
            var list = new List<int>();
            for (var i = 0; i < items; ++i)
            {
                list.Add(i);
            }
            var x = new Derivate(list);
            // assert no operation call
            x.AssertCounterValues(0, 0, 0, 0, 0);
        }

        [Fact]
        public void InsertTest()
        {
            var x = new Derivate(new[] {0, 2});
            x.Insert(1, 1);
            // assert single insert call
            x.AssertCounterValues(0, 1, 0, 0, 0);
        }

        [Fact]
        public void MoveTest()
        {
            var x = new Derivate(new[] {1, 2});
            x.Move(0, 1);
            // assert single move call
            x.AssertCounterValues(0, 0, 1, 0, 0);
        }

        [Fact]
        public void RemoveTest()
        {
            var x = new Derivate(new[] {0, 1, 2});
            x.Remove(0);
            // assert single remove call
            x.AssertCounterValues(0, 0, 0, 1, 0);
        }

        [Fact]
        public void SetAtUnexistingIndexFailTest()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new Derivate {[0] = 10});
        }

        [Fact]
        public void SetTest()
        {
            var x = new Derivate(new[] {0}) {[0] = 10};
            // assert single set call
            x.AssertCounterValues(0, 0, 0, 0, 1);
        }
    }

    internal class Derivate : ObservableCollection<int>
    {
        public Derivate()
            : this(new List<int>())
        {
        }

        public Derivate(IEnumerable<int> initialList)
            : base(initialList)
        {
            ClearItemCallCounter = 0;
            InsertItemCallCounter = 0;
            MoveItemCallCounter = 0;
            RemoveItemCallCounter = 0;
            SetItemCallCounter = 0;
        }

        public int ClearItemCallCounter { get; private set; }

        public int InsertItemCallCounter { get; private set; }

        public int MoveItemCallCounter { get; private set; }

        public int RemoveItemCallCounter { get; private set; }

        public int SetItemCallCounter { get; private set; }

        protected override void ClearItems()
        {
            base.ClearItems();
            ++ClearItemCallCounter;
        }

        protected override void InsertItem(int index, int item)
        {
            base.InsertItem(index, item);
            ++InsertItemCallCounter;
        }

        protected override void MoveItem(int oldIndex, int newIndex)
        {
            base.MoveItem(oldIndex, newIndex);
            ++MoveItemCallCounter;
        }

        protected override void RemoveItem(int index)
        {
            base.RemoveItem(index);
            ++RemoveItemCallCounter;
        }

        protected override void SetItem(int index, int item)
        {
            base.SetItem(index, item);
            ++SetItemCallCounter;
        }

        public void AssertCounterValues(int clear, int insert, int move, int remove, int set)
        {
            Assert.Equal(clear, ClearItemCallCounter);
            Assert.Equal(insert, InsertItemCallCounter);
            Assert.Equal(move, MoveItemCallCounter);
            Assert.Equal(remove, RemoveItemCallCounter);
            Assert.Equal(set, SetItemCallCounter);
        }
    }
}
