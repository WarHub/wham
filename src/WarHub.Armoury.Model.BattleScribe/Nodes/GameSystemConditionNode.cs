namespace WarHub.Armoury.Model.BattleScribe.Nodes
{
    using System;
    using System.Collections.Generic;
    using BattleScribeXml;

    internal class GameSystemConditionNode
        : XmlBackedNodeSimple<IGameSystemCondition, GameSystemCondition, Condition,
            IGameSystemItem>
    {
        public GameSystemConditionNode(Func<IList<Condition>> listGet,
            IGameSystemItem parent)
            : base(parent, listGet, Transformation, Factory)
        {
        }

        protected override void ProcessItemAddition(IGameSystemCondition item)
        {
            item.Context = Parent.Context;
        }

        protected override void ProcessItemRemoval(IGameSystemCondition item)
        {
            item.Context = null;
        }

        private static GameSystemCondition Factory()
        {
            var condition = new Condition();
            return Transformation(condition);
        }

        private static GameSystemCondition Transformation(Condition arg)
        {
            return new GameSystemCondition(arg);
        }
    }
}
