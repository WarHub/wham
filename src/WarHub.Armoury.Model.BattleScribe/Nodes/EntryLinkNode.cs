// WarHub licenses this file to you under the MIT license.
// See LICENSE file in the project root for more information.

namespace WarHub.Armoury.Model.BattleScribe.Nodes
{
    using System;
    using System.Collections.Generic;
    using BattleScribeXml;

    internal class EntryLinkNode
        : XmlBackedNode<IEntryLink, EntryLink, Link, ICatalogueItem, IEntry>
    {
        public EntryLinkNode(Func<IList<Link>> listGet, ICatalogueItem parent)
            : base(parent, listGet, Transformation, Factory)
        {
        }

        protected override void ProcessItemAddition(IEntryLink item)
        {
            item.Context = Parent.Context;
        }

        protected override void ProcessItemRemoval(IEntryLink item)
        {
            item.Context = null;
        }

        private static EntryLink Factory(IEntry entry)
        {
            var link = new Link
            {
                LinkType = LinkType.Entry,
                TargetGuid = entry.Id.Value
            };
            link.SetNewGuid();
            return Transformation(link);
        }

        private static EntryLink Transformation(Link arg)
        {
            return new EntryLink(arg);
        }
    }
}
