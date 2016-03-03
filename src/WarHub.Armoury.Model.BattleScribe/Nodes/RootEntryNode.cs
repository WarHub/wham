namespace WarHub.Armoury.Model.BattleScribe.Nodes
{
    using System;
    using System.Collections.Generic;
    using BattleScribeXml;
    using ICatalogue = Model.ICatalogue;

    internal class RootEntryNode
        : XmlBackedNodeSimple<IRootEntry, RootEntry, Entry, ICatalogue>
    {
        public RootEntryNode(Func<IList<Entry>> listGet, ICatalogue parent)
            : base(parent, listGet, Transformation, Factory)
        {
        }

        protected override void ProcessItemAddition(IRootEntry item)
        {
            item.Context = Parent.Context;
        }

        protected override void ProcessItemRemoval(IRootEntry item)
        {
            item.Context = null;
        }

        private static RootEntry Factory()
        {
            var xml = new Entry
            {
                CategoryGuid = ReservedIdentifiers.NoCategoryId
            };
            xml.SetNewGuid();
            return Transformation(xml);
        }

        private static RootEntry Transformation(Entry arg)
        {
            return new RootEntry(arg);
        }
    }
}
