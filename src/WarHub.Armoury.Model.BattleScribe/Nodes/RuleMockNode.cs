// WarHub licenses this file to you under the MIT license.
// See LICENSE file in the project root for more information.

namespace WarHub.Armoury.Model.BattleScribe.Nodes
{
    using System;
    using System.Collections.Generic;

    internal class RuleMockNode
        : XmlBackedObservableCollection<IRuleMock, RuleMock, BattleScribeXml.RuleMock, IForceContextProvider>
    {
        public RuleMockNode(Func<IList<BattleScribeXml.RuleMock>> listGet, IForceContextProvider parent)
            : base(parent, listGet, Transformation)
        {
        }

        protected override void ProcessItemAddition(IRuleMock item)
        {
            item.ForceContext = Parent.ForceContext;
        }

        protected override void ProcessItemRemoval(IRuleMock item)
        {
            item.ForceContext = null;
        }

        private static RuleMock Transformation(BattleScribeXml.RuleMock arg)
        {
            return new RuleMock(arg);
        }
    }
}
