// WarHub licenses this file to you under the MIT license.
// See LICENSE file in the project root for more information.

namespace WarHub.Armoury.Model
{
    using System;
    using System.Collections.ObjectModel;
    using System.ComponentModel;

    public class NodeSimple<T> : ObservableCollection<T>, INodeSimple<T>
        where T : INotifyPropertyChanged
    {
        public NodeSimple(Func<T> factoryFunc)
        {
            if (factoryFunc == null)
                throw new ArgumentNullException(nameof(factoryFunc));
            FactoryFunc = factoryFunc;
        }

        protected virtual Func<T> FactoryFunc { get; }

        public virtual T AddNew()
        {
            var newItem = FactoryFunc();
            Add(newItem);
            return newItem;
        }

        public virtual T InsertNew(int index)
        {
            var newItem = FactoryFunc();
            Insert(index, newItem);
            return newItem;
        }
    }
}
