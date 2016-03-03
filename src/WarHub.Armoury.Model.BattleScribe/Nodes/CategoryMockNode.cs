namespace WarHub.Armoury.Model.BattleScribe.Nodes
{
    using System;
    using System.Collections.Generic;
    using BattleScribeXml;
    using CategoryMock = BattleScribe.CategoryMock;
    using Force = BattleScribe.Force;

    internal class CategoryMockNode
        : XmlBackedNode<ICategoryMock, CategoryMock, BattleScribeXml.CategoryMock, Force, ICategory>
    {
        public CategoryMockNode(Func<IList<BattleScribeXml.CategoryMock>> listGet, Force parent)
            : base(parent, listGet, Transformation, Factory)
        {
        }

        protected override void ProcessItemAddition(ICategoryMock item)
        {
            item.ForceContext = Parent.ForceContext;
        }

        protected override void ProcessItemRemoval(ICategoryMock item)
        {
            item.ForceContext = null;
        }

        private static CategoryMock Factory(ICategory category)
        {
            var xmlMock = new BattleScribeXml.CategoryMock
            {
                CategoryGuid = category.Id.Value,
                CategoryId = category.Id.RawValue,
                Name = category.Name
            };
            IdentifiedExtensions.SetNewGuid(xmlMock);
            return Transformation(xmlMock);
        }

        private static CategoryMock Transformation(BattleScribeXml.CategoryMock arg)
        {
            return new CategoryMock(arg);
        }
    }
}
