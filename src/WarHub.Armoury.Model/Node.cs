// WarHub licenses this file to you under the MIT license.
// See LICENSE file in the project root for more information.

namespace WarHub.Armoury.Model
{
    using System;
    using System.ComponentModel;

    public class Node<TItem, TFactoryArg> : ObservableList<TItem>, INode<TItem, TFactoryArg>
        where TItem : INotifyPropertyChanged
    {
        public Node(Func<TFactoryArg, TItem> factoryFunc)
        {
            if (factoryFunc == null)
                throw new ArgumentNullException(nameof(factoryFunc));
            FactoryFunc = factoryFunc;
        }

        protected virtual Func<TFactoryArg, TItem> FactoryFunc { get; }

        public virtual TItem AddNew(TFactoryArg arg)
        {
            var item = FactoryFunc(arg);
            Add(item);
            return item;
        }

        public virtual TItem InsertNew(int index, TFactoryArg arg)
        {
            var item = FactoryFunc(arg);
            Insert(index, item);
            return item;
        }
    }
}
