namespace WarHub.Armoury.Model.Builders.Implementations
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class RuleBuilder : BuilderCore, IRuleBuilder
    {
        public RuleBuilder(RuleLinkPair ruleLinkPair, IBuilderCore parentBuilder,
            IBuilderAncestorContext ancestorContext)
            : base(parentBuilder, ancestorContext)
        {
            if (ruleLinkPair == null) throw new ArgumentNullException(nameof(ruleLinkPair));
            RuleLinkPair = ruleLinkPair;
        }

        public IApplicableVisibility ApplicableVisibility { get; } = new ApplicableVisibility();
        public RuleLinkPair RuleLinkPair { get; }
        public string ApplicableName { get; set; }
        public string ApplicableDescription { get; set; }
        public override IStatAggregate StatAggregate { get; } = new RuleStatAggregate();
        public override bool IsForEntityId(Guid idValue) => RuleLinkPair.AnyHasId(idValue);

        public override void ApplyModifiers()
        {
            var modifiers = !RuleLinkPair.HasLink
                ? RuleLinkPair.Rule.Modifiers
                : RuleLinkPair.Rule.Modifiers.Concat(RuleLinkPair.Link.Modifiers);
            //TODO:
            //this.Apply(modifiers, new RuleConditionResolver(this));
        }

        private class RuleStatAggregate : IStatAggregate
        {
            public IEnumerable<IStatAggregate> ChildrenAggregates => Enumerable.Empty<IStatAggregate>();
            public uint ChildSelectionsCount => 0;
            public decimal PointsTotal => 0;
            public decimal GetPointsTotal(Guid nodeGuid) => 0;
            public uint GetSelectionCount(Guid selectionGuid) => 0;
        }
    }
}
