namespace WarHub.Armoury.Model.Builders.Implementations
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using ConditionResolvers;
    using EntryTree;
    using ModifierAppliers;

    public class CategoryBuilder : BuilderCore, ICategoryBuilder
    {
        public CategoryBuilder(ICategoryMock categoryMock, IBuilderCore parentBuilder,
            IBuilderAncestorContext ancestorContext) : base(parentBuilder, ancestorContext)
        {
            if (categoryMock == null) throw new ArgumentNullException(nameof(categoryMock));
            if (parentBuilder == null) throw new ArgumentNullException(nameof(parentBuilder));
            if (ancestorContext == null) throw new ArgumentNullException(nameof(ancestorContext));
            CategoryMock = categoryMock;
            InnerApplicableGeneralLimits.CopyFrom(Category.Limits);
            EntryBuilders = CreateChildren(categoryMock, this, ancestorContext);
            StatAggregate = new CategoryStatAggregate(this);
        }

        private GeneralLimits InnerApplicableGeneralLimits { get; } = new GeneralLimits();
        private ICategory Category => CategoryMock.CategoryLink.Target;
        public ILimits<int, decimal, int> ApplicableGeneralLimits => InnerApplicableGeneralLimits;
        public override IStatAggregate StatAggregate { get; }
        public override bool IsForEntityId(Guid idValue) => Category.IdValueEquals(idValue);

        public override void ApplyModifiers()
        {
            foreach (var builderCore in Children)
            {
                builderCore.ApplyModifiers();
            }
            var newLimits = new GeneralLimits();
            newLimits.CopyFrom(Category.Limits);
            newLimits.Apply(Category.CategoryModifiers, new CategoryConditionResolver(this));
            InnerApplicableGeneralLimits.CopyFrom(newLimits);
        }

        public override IEnumerable<IBuilderCore> Children => EntryBuilders;
        public ICategoryMock CategoryMock { get; }
        public IEnumerable<IEntryBuilder> EntryBuilders { get; }

        private static IEnumerable<IEntryBuilder> CreateChildren(ICategoryMock categoryMock, ICategoryBuilder @this,
            IBuilderAncestorContext ancestorContext)
        {
            var childrenContext = ancestorContext.AppendedWith(@this);
            var catalogue = categoryMock.ForceContext.Force.CatalogueLink.Target;
            var entryTreeRoot = TreeRoot.Create(catalogue, categoryMock.CategoryLink.Target);
            var selectionsMapping = entryTreeRoot.MapSelections(categoryMock);
            return
                entryTreeRoot.EntryNodes.Select(
                    node => new EntryBuilder(node, selectionsMapping[node], @this, childrenContext)).ToArray();
        }

        private class CategoryStatAggregate : StatAggregateBase
        {
            public CategoryStatAggregate(IBuilderCore builder)
                : base(builder.Children.Select(core => core.StatAggregate))
            {
                //TODO include or not "count towards parent" appropriately - IMPORTANT
            }

            public override uint ChildSelectionsCount
                => ChildrenAggregates.Aggregate(0u, (sum, aggregate) => sum + aggregate.ChildSelectionsCount);

            public override decimal PointsTotal
                => ChildrenAggregates.Aggregate(0m, (sum, aggregate) => sum + aggregate.PointsTotal);

            protected override decimal GetChildPointsValue(Guid nodeGuid) => 0m;

            protected override uint GetChildSelectionCount(Guid selectionGuid) => 0u;
        }
    }
}
