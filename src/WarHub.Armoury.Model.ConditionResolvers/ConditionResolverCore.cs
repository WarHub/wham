// WarHub licenses this file to you under the MIT license.
// See LICENSE file in the project root for more information.

namespace WarHub.Armoury.Model.ConditionResolvers
{
    using System;
    using System.ComponentModel;
    using Builders;

    public class ConditionResolverCore<TBuilder> : IConditionResolver where TBuilder : IBuilderCore
    {
        public ConditionResolverCore(ExtractChildValue<TBuilder> extract, TBuilder builder)
        {
            if (extract == null)
                throw new ArgumentNullException(nameof(extract));
            Extract = extract;
            Builder = builder;
        }

        private TBuilder Builder { get; }

        private ExtractChildValue<TBuilder> Extract { get; }

        public int CountRepeats(IRepetitionInfo repetition)
        {
            return !repetition.IsActive ? 0 : (int) repetition.Loops*GetRepeatsPerLoop(repetition);
        }

        public bool IsMet(ICondition condition)
        {
            return condition.SatisfiedBy(GetChildValue(condition));
        }

        private ConditionChildValue GetChildValue(ICondition condition)
        {
            return Extract(condition, Builder);
        }

        private int GetRepeatsPerLoop(IConditionCore conditionCore)
        {
            var condition = Wrap(conditionCore);
            return (int) Math.Truncate(GetChildValue(condition).Number/condition.ChildValue);
        }

        private static ICondition Wrap(IConditionCore conditionCore)
        {
            return new ConditionProxy(conditionCore);
        }

        private class ConditionProxy : ICondition
        {
            public ConditionProxy(IConditionCore @base)
            {
                Base = @base;
            }

            private IConditionCore Base { get; }

            public event PropertyChangedEventHandler PropertyChanged
            {
                add { Base.PropertyChanged += value; }
                remove { Base.PropertyChanged -= value; }
            }

            public ConditionChildKind ChildKind
            {
                get { return Base.ChildKind; }
                set { Base.ChildKind = value; }
            }

            public ILink ChildLink => Base.ChildLink;

            public decimal ChildValue
            {
                get { return Base.ChildValue; }
                set { Base.ChildValue = value; }
            }

            public ConditionValueUnit ChildValueUnit
            {
                get { return Base.ChildValueUnit; }
                set { Base.ChildValueUnit = value; }
            }

            public ConditionParentKind ParentKind
            {
                get { return Base.ParentKind; }
                set { Base.ParentKind = value; }
            }

            public ILink ParentLink => Base.ParentLink;

            /// <summary>
            ///     Clones object but assigns it new Guid (if exists).
            /// </summary>
            /// <returns>Deep copy of object (with new Guid).</returns>
            public ICondition Clone()
            {
                throw new NotSupportedException();
            }

            public ConditionKind ConditionKind
            {
                get { return ConditionKind.EqualTo; }
                set
                {
                    /*ignore, it's a proxy */
                }
            }
        }
    }
}
