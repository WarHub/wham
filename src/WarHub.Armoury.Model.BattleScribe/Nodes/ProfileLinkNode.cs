namespace WarHub.Armoury.Model.BattleScribe.Nodes
{
    using System;
    using System.Collections.Generic;
    using BattleScribeXml;

    internal class ProfileLinkNode
        : XmlBackedNode<IProfileLink, ProfileLink, Link, ICatalogueItem, IProfile>
    {
        public ProfileLinkNode(Func<IList<Link>> listGet, ICatalogueItem manager)
            : base(manager, listGet, Transformation, Factory)
        {
        }

        protected override void ProcessItemAddition(IProfileLink item)
        {
            item.Context = Parent.Context;
        }

        protected override void ProcessItemRemoval(IProfileLink item)
        {
            item.Context = null;
        }

        private static ProfileLink Factory(IProfile profile)
        {
            var link = new Link
            {
                LinkType = LinkType.Profile,
                TargetGuid = profile.Id.Value
            };
            link.SetNewGuid();
            return Transformation(link);
        }

        private static ProfileLink Transformation(Link arg)
        {
            return new ProfileLink(arg);
        }
    }
}
