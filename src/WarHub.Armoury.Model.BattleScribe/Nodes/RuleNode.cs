namespace WarHub.Armoury.Model.BattleScribe.Nodes
{
    using System;
    using System.Collections.Generic;
    using BattleScribeXml;
    using Rule = BattleScribe.Rule;

    internal class RuleNode : XmlBackedNode<IRule, Rule, BattleScribeXml.Rule, ICatalogueContextProvider, IEntry>,
        INodeSimple<IRule>
    {
        public RuleNode(Func<IList<BattleScribeXml.Rule>> listGet, ICatalogueContextProvider parent)
            : base(parent, listGet, Transformation, Factory)
        {
        }

        public IRule AddNew()
        {
            var newItem = Factory();
            Add(newItem);
            return newItem;
        }

        public IRule InsertNew(int index)
        {
            var newItem = Factory();
            Insert(index, newItem);
            return newItem;
        }

        protected override void ProcessItemAddition(IRule item)
        {
            item.Context = Parent.Context;
        }

        protected override void ProcessItemRemoval(IRule item)
        {
            item.Context = null;
        }

        private static Rule Factory(IEntry entry)
        {
            var xml = new BattleScribeXml.Rule
            {
                Name = entry.Name,
                Book = entry.Book.Title,
                Page = entry.Book.Page
            };
            IdentifiedExtensions.SetNewGuid(xml);
            return Transformation(xml);
        }

        private static Rule Transformation(BattleScribeXml.Rule arg)
        {
            return new Rule(arg);
        }

        private Rule Factory()
        {
            var xml = new BattleScribeXml.Rule();
            xml.Guid = Guid.NewGuid();
            return Transformation(xml);
        }
    }
}
