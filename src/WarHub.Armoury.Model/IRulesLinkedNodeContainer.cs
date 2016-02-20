// WarHub licenses this file to you under the MIT license.
// See LICENSE file in the project root for more information.

namespace WarHub.Armoury.Model
{
    public interface IRulesLinkedNodeContainer
    {
        INode<IRuleLink, IRule> RuleLinks { get; }

        INodeSimple<IRule> Rules { get; }
    }
}
