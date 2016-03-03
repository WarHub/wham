namespace WarHub.Armoury.Model.BattleScribe.Nodes
{
    using System;
    using System.Collections.Generic;
    using BattleScribeXml;

    internal class GroupNode
        : XmlBackedNodeSimple<IGroup, Group, EntryGroup, ICatalogueContextProvider>
    {
        public GroupNode(Func<IList<EntryGroup>> listGet, ICatalogueContextProvider parent)
            : base(parent, listGet, Transformation, Factory)
        {
        }

        protected override void ProcessItemAddition(IGroup item)
        {
            item.Context = Parent.Context;
        }

        protected override void ProcessItemRemoval(IGroup item)
        {
            item.Context = null;
        }

        private static Group Factory()
        {
            var xml = new EntryGroup();
            xml.SetNewGuid();
            return Transformation(xml);
        }

        private static Group Transformation(EntryGroup arg)
        {
            return new Group(arg);
        }
    }
}
