namespace WarHub.Armoury.Model.Builders
{
    using System.Collections.Generic;
    using EntryTree;

    public interface ISelectionBuilder : IBuilderCore, IEntryBuilderNode
    {
        INode EntryTreeRoot { get; }
        IEnumerable<IProfileBuilder> ProfileBuilders { get; }
        IEnumerable<IRuleBuilder> RuleBuilders { get; }
        ISelection Selection { get; }
    }
}
