namespace WarHub.Armoury.Model.BattleScribe.Nodes
{
    using System;
    using System.Collections.Generic;
    using BattleScribeXml;
    using ICatalogue = Model.ICatalogue;
    using XmlLink = BattleScribeXml.Link;

    internal class RootLinkNode
        : XmlBackedNode<IRootLink, RootLink, XmlLink, ICatalogue, IEntry>
    {
        public RootLinkNode(Func<IList<XmlLink>> listGet, ICatalogue parent)
            : base(parent, listGet, Transformation, Factory)
        {
        }

        protected override void ProcessItemAddition(IRootLink item)
        {
            item.Context = Parent.Context;
        }

        protected override void ProcessItemRemoval(IRootLink item)
        {
            item.Context = null;
        }

        private static RootLink Factory(IEntry entry)
        {
            var link = new XmlLink
            {
                LinkType = LinkType.Entry,
                TargetGuid = entry.Id.Value,
                CategoryGuid = ReservedIdentifiers.NoCategoryId
            };
            link.SetNewGuid();
            return Transformation(link);
        }

        private static RootLink Transformation(XmlLink arg)
        {
            return new RootLink(arg);
        }
    }
}
