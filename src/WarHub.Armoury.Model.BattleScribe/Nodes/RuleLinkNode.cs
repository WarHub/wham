// WarHub licenses this file to you under the MIT license.
// See LICENSE file in the project root for more information.

namespace WarHub.Armoury.Model.BattleScribe.Nodes
{
    using System;
    using System.Collections.Generic;
    using BattleScribeXml;

    internal class RuleLinkNode
        : XmlBackedNode<IRuleLink, RuleLink, Link, ICatalogueContextProvider, IRule>
    {
        public RuleLinkNode(Func<IList<Link>> listGet, ICatalogueContextProvider parent)
            : base(parent, listGet, Transformation, Factory)
        {
        }

        protected override void ProcessItemAddition(IRuleLink item)
        {
            item.Context = Parent.Context;
        }

        protected override void ProcessItemRemoval(IRuleLink item)
        {
            item.Context = null;
        }

        private static RuleLink Factory(IRule rule)
        {
            var link = new Link
            {
                LinkType = LinkType.Rule,
                TargetGuid = rule.Id.Value
            };
            link.SetNewGuid();
            return Transformation(link);
        }

        private static RuleLink Transformation(Link arg)
        {
            return new RuleLink(arg);
        }
    }
}
