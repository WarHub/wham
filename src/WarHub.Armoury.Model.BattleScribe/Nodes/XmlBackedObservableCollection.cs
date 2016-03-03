namespace WarHub.Armoury.Model.BattleScribe.Nodes
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Linq;
    using BattleScribeXml.GuidMapping;

    /// <summary>
    ///     Manages collection of objects simultaneously mirroring changes into backing list of xml
    ///     backend objects.
    /// </summary>
    /// <remarks>
    ///     Monitors only changes made to the collection - the backing xml list shouldn't be modified
    ///     outside of this class.
    /// </remarks>
    /// <typeparam name="TInterface">Type of objects in this collection (the visible one).</typeparam>
    /// <typeparam name="TImpl">
    ///     Implementation type of the objects in this collection - using other types when ie. adding
    ///     will result in exception.
    /// </typeparam>
    /// <typeparam name="TXml">Type of objects in the backing xml object list.</typeparam>
    /// <typeparam name="TParent">Type of object owning this node.</typeparam>
    internal class XmlBackedObservableCollection<TInterface, TImpl, TXml, TParent>
        : XmlBackedObservableCollection<TInterface, TImpl, TXml>
        where TInterface : INotifyPropertyChanged
        where TImpl : class, IXmlBackedObject<TXml>, TInterface
        where TParent : class
    {
        public XmlBackedObservableCollection(
            TParent parent,
            Func<IList<TXml>> xmlListGet,
            Func<TXml, TParent, TImpl> transformation)
            : this(parent, xmlListGet, x => transformation(x, parent))
        {
        }

        public XmlBackedObservableCollection(
            TParent parent,
            Func<IList<TXml>> xmlListGet,
            Func<TXml, TImpl> transformation)
            : base(xmlListGet, transformation)
        {
            Parent = parent;
        }

        public TParent Parent { get; }

        protected override void ClearItems()
        {
            var items = this.ToList();
            base.ClearItems();
            foreach (var item in items)
            {
                ProcessItemRemoval(item);
            }
        }

        protected override void InsertItem(int index, TInterface item)
        {
            base.InsertItem(index, item);
            ProcessItemAddition(item);
        }

        /// <summary>
        ///     Called after new item was added to collection.
        /// </summary>
        /// <param name="item">New item in collection.</param>
        protected virtual void ProcessItemAddition(TInterface item)
        {
        }

        /// <summary>
        ///     Called after an item was removed from collection.
        /// </summary>
        /// <param name="item">An item removed from collection.</param>
        protected virtual void ProcessItemRemoval(TInterface item)
        {
        }

        protected override void RemoveItem(int index)
        {
            var item = this[index];
            base.RemoveItem(index);
            ProcessItemRemoval(item);
        }

        protected override void SetItem(int index, TInterface item)
        {
            var oldItem = this[index];
            base.SetItem(index, item);
            ProcessItemRemoval(oldItem);
            ProcessItemAddition(item);
        }
    }

    /// <summary>
    ///     Manages collection of objects simultaneously mirroring changes into backing list of xml
    ///     backend objects.
    /// </summary>
    /// <remarks>
    ///     Monitors only changes made to the collection - the backing xml list shouldn't be modified
    ///     outside of this class.
    /// </remarks>
    /// <typeparam name="TInterface">Type of objects in this collection (the visible one).</typeparam>
    /// <typeparam name="TImpl">
    ///     Implementation type of the objects in this collection - using other types when ie. adding
    ///     will result in exception.
    /// </typeparam>
    /// <typeparam name="TXml">Type of objects in the backing xml object list.</typeparam>
    internal class XmlBackedObservableCollection<TInterface, TImpl, TXml>
        : ObservableCollection<TInterface>, IObservableList<TInterface>
        where TInterface : INotifyPropertyChanged
        where TImpl : class, IXmlBackedObject<TXml>, TInterface
    {
        private readonly Func<IList<TXml>> _xmlListGet;

        public XmlBackedObservableCollection(
            Func<IList<TXml>> xmlListGet,
            Func<TXml, TImpl> transformation)
            : base(xmlListGet().Select(transformation))
        {
            _xmlListGet = xmlListGet;
        }

        public GuidController Controller { get; set; }

        protected IList<TXml> XmlList
        {
            get { return _xmlListGet(); }
        }

        protected override void ClearItems()
        {
            base.ClearItems();
            XmlList.Clear();
        }

        protected override void InsertItem(int index, TInterface item)
        {
            ThrowForIllegalClassArgument(item);
            base.InsertItem(index, item);
            XmlList.Insert(index, ((TImpl) item).XmlBackend);
        }

        protected override void MoveItem(int oldIndex, int newIndex)
        {
            base.MoveItem(oldIndex, newIndex);
            var list = XmlList;
            list.RemoveAt(oldIndex);
            list.Insert(newIndex, list[newIndex]);
        }

        protected override void RemoveItem(int index)
        {
            base.RemoveItem(index);
            XmlList.RemoveAt(index);
        }

        protected override void SetItem(int index, TInterface item)
        {
            ThrowForIllegalClassArgument(item);
            base.SetItem(index, item);
            XmlList[index] = ((TImpl) item).XmlBackend;
        }

        protected void ThrowForIllegalClassArgument(TInterface item)
        {
            if (!(item is TImpl))
            {
                var msg = string.Format("{0}\n{1} {2} {3}\n{4} {5}.",
                    "Argument not an instance of the correct class.",
                    "Object was of", item.GetType(), "type,",
                    "expected", typeof(TImpl));
                throw new ArgumentException(msg, "item");
            }
        }
    }
}
