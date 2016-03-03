// WarHub licenses this file to you under the MIT license.
// See LICENSE file in the project root for more information.

namespace WarHub.Armoury.Model.BattleScribe.Nodes
{
    using System;
    using System.Collections.Generic;
    using BattleScribeXml;

    internal class GameSystemConditionGroupNode
        : XmlBackedNodeSimple<IGameSystemConditionGroup, GameSystemConditionGroup,
            ConditionGroup, IGameSystemItem>
    {
        public GameSystemConditionGroupNode(Func<IList<ConditionGroup>> listGet,
            IGameSystemItem parent)
            : base(parent, listGet, Transformation, Factory)
        {
        }

        protected override void ProcessItemAddition(IGameSystemConditionGroup item)
        {
            item.Context = Parent.Context;
        }

        protected override void ProcessItemRemoval(IGameSystemConditionGroup item)
        {
            item.Context = null;
        }

        private static GameSystemConditionGroup Factory()
        {
            var condition = new ConditionGroup();
            return Transformation(condition);
        }

        private static GameSystemConditionGroup Transformation(ConditionGroup arg)
        {
            return new GameSystemConditionGroup(arg);
        }
    }
}
