// WarHub licenses this file to you under the MIT license.
// See LICENSE file in the project root for more information.

namespace WarHub.Armoury.Model.BattleScribe.Nodes
{
    using System;
    using System.Collections.Generic;
    using BattleScribeXml;
    using Entry = BattleScribe.Entry;

    internal class EntryNode
        : XmlBackedNodeSimple<IEntry, Entry, BattleScribeXml.Entry, ICatalogueContextProvider>
    {
        public EntryNode(Func<IList<BattleScribeXml.Entry>> listGet, ICatalogueContextProvider parent)
            : base(parent, listGet, Transformation, Factory)
        {
        }

        protected override void ProcessItemAddition(IEntry item)
        {
            item.Context = Parent.Context;
        }

        protected override void ProcessItemRemoval(IEntry item)
        {
            item.Context = null;
        }

        private static Entry Factory()
        {
            var xml = new BattleScribeXml.Entry();
            xml.SetNewGuid();
            return Transformation(xml);
        }

        private static Entry Transformation(BattleScribeXml.Entry arg)
        {
            return new Entry(arg);
        }
    }
}
