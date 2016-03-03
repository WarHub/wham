// WarHub licenses this file to you under the MIT license.
// See LICENSE file in the project root for more information.

namespace WarHub.Armoury.Model.BattleScribe.Nodes
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using BattleScribeXml.GuidMapping;

    internal class XmlBackedNode<TInterface, TImpl, TXml, TParent, TArg>
        : XmlBackedObservableCollection<TInterface, TImpl, TXml, TParent>,
            INode<TInterface, TArg>
        where TInterface : INotifyPropertyChanged
        where TImpl : class, IXmlBackedObject<TXml>, TInterface
        where TXml : IGuidControllable
        where TParent : class
    {
        public XmlBackedNode(
            TParent parent,
            Func<IList<TXml>> xmlListGet,
            Func<TXml, TParent, TImpl> transformation,
            Func<TArg, TParent, TImpl> factory)
            : base(parent, xmlListGet, transformation)
        {
            FactoryFunc = x => factory(x, parent);
        }

        public XmlBackedNode(
            TParent parent,
            Func<IList<TXml>> xmlListGet,
            Func<TXml, TImpl> transformation,
            Func<TArg, TImpl> factory)
            : base(parent, xmlListGet, transformation)
        {
            FactoryFunc = factory;
        }

        public XmlBackedNode(
            TParent parent,
            IList<TXml> xmlList,
            Func<TXml, TImpl> transformation,
            Func<TArg, TImpl> factory)
            : base(parent, () => xmlList, transformation)
        {
            FactoryFunc = factory;
        }

        protected Func<TArg, TImpl> FactoryFunc { get; }

        public TInterface AddNew(TArg arg)
        {
            var newItem = FactoryFunc(arg);
            if (Controller != null)
            {
                newItem.XmlBackend.Process(Controller);
            }
            Add(newItem);
            return newItem;
        }

        public TInterface InsertNew(int index, TArg arg)
        {
            var newItem = FactoryFunc(arg);
            if (Controller != null)
            {
                newItem.XmlBackend.Process(Controller);
            }
            Insert(index, newItem);
            return newItem;
        }
    }
}
