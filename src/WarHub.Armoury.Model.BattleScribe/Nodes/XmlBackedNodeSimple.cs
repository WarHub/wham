// WarHub licenses this file to you under the MIT license.
// See LICENSE file in the project root for more information.

namespace WarHub.Armoury.Model.BattleScribe.Nodes
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using BattleScribeXml.GuidMapping;

    internal class XmlBackedNodeSimple<TInterface, TImpl, TXml, TParent>
        : XmlBackedObservableCollection<TInterface, TImpl, TXml, TParent>,
            INodeSimple<TInterface>
        where TInterface : INotifyPropertyChanged
        where TImpl : class, IXmlBackedObject<TXml>, TInterface
        where TXml : IGuidControllable
        where TParent : class
    {
        public XmlBackedNodeSimple(
            TParent parent,
            Func<IList<TXml>> xmlListGet,
            Func<TXml, TParent, TImpl> transformation,
            Func<TParent, TImpl> factory)
            : base(parent, xmlListGet, transformation)
        {
            FactoryFunc = factory;
        }

        public XmlBackedNodeSimple(
            TParent parent,
            Func<IList<TXml>> xmlListGet,
            Func<TXml, TImpl> transformation,
            Func<TImpl> factory)
            : base(parent, xmlListGet, transformation)
        {
            FactoryFunc = _ => factory();
        }

        protected Func<TParent, TImpl> FactoryFunc { get; }

        public TInterface AddNew()
        {
            var newItem = FactoryFunc(Parent);
            if (Controller != null)
            {
                newItem.XmlBackend.Process(Controller);
            }
            Add(newItem);
            return newItem;
        }

        public TInterface InsertNew(int index)
        {
            var newItem = FactoryFunc(Parent);
            if (Controller != null)
            {
                newItem.XmlBackend.Process(Controller);
            }
            Insert(index, newItem);
            return newItem;
        }
    }

    internal class XmlBackedNodeSimple<TInterface, TImpl, TXml>
        : XmlBackedObservableCollection<TInterface, TImpl, TXml>,
            INodeSimple<TInterface>
        where TInterface : INotifyPropertyChanged
        where TImpl : class, IXmlBackedObject<TXml>, TInterface
        where TXml : IGuidControllable
    {
        public XmlBackedNodeSimple(
            Func<IList<TXml>> xmlListGet,
            Func<TXml, TImpl> transformation,
            Func<TImpl> factory)
            : base(xmlListGet, transformation)
        {
            FactoryFunc = factory;
        }

        public XmlBackedNodeSimple(
            IList<TXml> xmlList,
            Func<TXml, TImpl> transformation,
            Func<TImpl> factory)
            : this(() => xmlList, transformation, factory)
        {
        }

        protected Func<TImpl> FactoryFunc { get; }

        public TInterface AddNew()
        {
            var newItem = FactoryFunc();
            if (Controller != null)
            {
                newItem.XmlBackend.Process(Controller);
            }
            Add(newItem);
            return newItem;
        }

        public TInterface InsertNew(int index)
        {
            var newItem = FactoryFunc();
            if (Controller != null)
            {
                newItem.XmlBackend.Process(Controller);
            }
            Insert(index, newItem);
            return newItem;
        }
    }
}
