// WarHub licenses this file to you under the MIT license.
// See LICENSE file in the project root for more information.

namespace WarHub.Armoury.Model.BattleScribe.Nodes
{
    using System;
    using System.Collections.Generic;
    using BattleScribeXml;

    internal class GroupLinkNode
        : XmlBackedNode<IGroupLink, GroupLink, Link, ICatalogueItem, IGroup>
    {
        public GroupLinkNode(Func<IList<Link>> listGet, ICatalogueItem parent)
            : base(parent, listGet, Transformation, Factory)
        {
        }

        protected override void ProcessItemAddition(IGroupLink item)
        {
            item.Context = Parent.Context;
        }

        protected override void ProcessItemRemoval(IGroupLink item)
        {
            item.Context = null;
        }

        private static GroupLink Factory(IGroup group)
        {
            var link = new Link
            {
                LinkType = LinkType.EntryGroup,
                TargetGuid = @group.Id.Value
            };
            link.SetNewGuid();
            return Transformation(link);
        }

        private static GroupLink Transformation(Link arg)
        {
            return new GroupLink(arg);
        }
    }
}
