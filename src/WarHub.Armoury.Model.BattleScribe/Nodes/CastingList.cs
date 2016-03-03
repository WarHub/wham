// WarHub licenses this file to you under the MIT license.
// See LICENSE file in the project root for more information.

namespace WarHub.Armoury.Model.BattleScribe.Nodes
{
    using System;
    using System.Collections;
    using System.Collections.Generic;

    /// <summary>
    ///     Performs internal casting every time any method is called, and calls these methods on
    ///     internal list provided in constructor.
    /// </summary>
    /// <typeparam name="TInterface"></typeparam>
    /// <typeparam name="TImpl"></typeparam>
    internal class CastingList<TInterface, TImpl> : IList<TInterface>
        where TImpl : TInterface
    {
        private readonly Func<IList<TImpl>> _baseListGet;

        public CastingList(Func<IList<TImpl>> baseListGet)
        {
            _baseListGet = baseListGet;
        }

        public int IndexOf(TInterface item)
        {
            return _baseListGet().IndexOf((TImpl) item);
        }

        public void Insert(int index, TInterface item)
        {
            _baseListGet().Insert(index, (TImpl) item);
        }

        public void RemoveAt(int index)
        {
            _baseListGet().RemoveAt(index);
        }

        public TInterface this[int index]
        {
            get { return _baseListGet()[index]; }
            set { _baseListGet()[index] = (TImpl) value; }
        }

        public void Add(TInterface item)
        {
            _baseListGet().Add((TImpl) item);
        }

        public void Clear()
        {
            _baseListGet().Clear();
        }

        public bool Contains(TInterface item)
        {
            return _baseListGet().Contains((TImpl) item);
        }

        public void CopyTo(TInterface[] array, int arrayIndex)
        {
            foreach (var item in _baseListGet())
            {
                array[arrayIndex++] = item;
            }
        }

        public int Count
        {
            get { return _baseListGet().Count; }
        }

        public bool IsReadOnly
        {
            get { return _baseListGet().IsReadOnly; }
        }

        public bool Remove(TInterface item)
        {
            return _baseListGet().Remove((TImpl) item);
        }

        public IEnumerator<TInterface> GetEnumerator()
        {
            foreach (TInterface item in _baseListGet())
            {
                yield return item;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _baseListGet().GetEnumerator();
        }
    }
}
