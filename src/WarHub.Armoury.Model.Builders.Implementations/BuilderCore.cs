// WarHub licenses this file to you under the MIT license.
// See LICENSE file in the project root for more information.

namespace WarHub.Armoury.Model.Builders.Implementations
{
    using System;
    using System.Collections.Generic;

    public abstract class BuilderCore : IBuilderCore
    {
        protected BuilderCore(IBuilderCore parentBuilder, IBuilderAncestorContext ancestorContext)
        {
            ParentBuilder = parentBuilder;
            AncestorContext = ancestorContext;
        }

        public IBuilderAncestorContext AncestorContext { get; }

        public abstract bool IsForEntityId(Guid idValue);
        public abstract void ApplyModifiers();

        public virtual IEnumerable<IBuilderCore> Children
        {
            get { yield break; }
        } //TODO change to abstract?

        public IBuilderCore ParentBuilder { get; }

        public abstract IStatAggregate StatAggregate { get; }
    }
}
