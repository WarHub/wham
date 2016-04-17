// WarHub licenses this file to you under the MIT license.
// See LICENSE file in the project root for more information.

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
