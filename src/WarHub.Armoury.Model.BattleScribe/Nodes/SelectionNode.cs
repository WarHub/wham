// WarHub licenses this file to you under the MIT license.
// See LICENSE file in the project root for more information.

namespace WarHub.Armoury.Model.BattleScribe.Nodes
{
    using System;
    using System.Collections.Generic;

    internal class SelectionNode
        : XmlBackedNode<ISelection, Selection, BattleScribeXml.Selection, IForceItem, CataloguePath>
    {
        public SelectionNode(Func<IList<BattleScribeXml.Selection>> listGet, IForceItem parent)
            : base(parent, listGet, (xml, p) => new Selection(xml), Factory)
        {
        }

        protected override void ProcessItemAddition(ISelection item)
        {
            item.ForceContext = Parent.ForceContext;
        }

        protected override void ProcessItemRemoval(ISelection item)
        {
            item.ForceContext = null;
        }

        private static Selection Factory(CataloguePath arg, IForceItem parent)
        {
            var selection = Selection.CreateFrom(arg);
            return selection;
        }
    }
}
