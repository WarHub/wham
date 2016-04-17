namespace WarHub.Armoury.Model.Builders
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    ///     Represents core abilities and properties of any node in builder tree structure. It also provides ability to simply
    ///     enumerate all children builders.
    /// </summary>
    public interface IBuilderCore
    {
        IBuilderAncestorContext AncestorContext { get; }
        IEnumerable<IBuilderCore> Children { get; }
        IBuilderCore ParentBuilder { get; }
        IStatAggregate StatAggregate { get; }

        bool IsForEntityId(Guid idValue);

        void ApplyModifiers();
    }
}
