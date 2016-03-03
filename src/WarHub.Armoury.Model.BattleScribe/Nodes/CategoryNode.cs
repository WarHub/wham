namespace WarHub.Armoury.Model.BattleScribe.Nodes
{
    using System;
    using System.Collections.Generic;
    using BattleScribeXml;
    using Category = BattleScribe.Category;
    using ForceType = BattleScribe.ForceType;

    internal class CategoryNode
        : XmlBackedNodeSimple<ICategory, Category, BattleScribeXml.Category, ForceType>
    {
        public CategoryNode(Func<IList<BattleScribeXml.Category>> listGet, ForceType parent)
            : base(parent, listGet, Transformation, Factory)
        {
        }

        protected override void ProcessItemAddition(ICategory item)
        {
            item.Context = Parent.Context;
        }

        protected override void ProcessItemRemoval(ICategory item)
        {
            item.Context = null;
        }

        private static Category Factory()
        {
            var xml = new BattleScribeXml.Category();
            IdentifiedExtensions.SetNewGuid(xml);
            return Transformation(xml);
        }

        private static Category Transformation(BattleScribeXml.Category arg)
        {
            return new Category(arg);
        }
    }
}
