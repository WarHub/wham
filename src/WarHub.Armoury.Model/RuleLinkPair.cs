// WarHub licenses this file to you under the MIT license.
// See LICENSE file in the project root for more information.

namespace WarHub.Armoury.Model
{
    using System;

    /// <summary>
    ///     Represents rule with optional link which pointed to it in source node.
    /// </summary>
    public class RuleLinkPair
    {
        private RuleLinkPair(IRule rule, IRuleLink link)
        {
            if (rule == null)
                throw new ArgumentNullException(nameof(rule));
            Rule = rule;
            Link = link;
        }

        /// <summary>
        ///     Gets if this pair contains <see cref="Link" />.
        /// </summary>
        public bool HasLink => Link != null;

        /// <summary>
        ///     Gets optional link which targets <see cref="Rule" />. May be null.
        /// </summary>
        public IRuleLink Link { get; }

        /// <summary>
        ///     Gets rule of this pair. Cannot be null.
        /// </summary>
        public IRule Rule { get; }

        public static RuleLinkPair From(IRule rule)
        {
            return new RuleLinkPair(rule, null);
        }

        public static RuleLinkPair From(IRuleLink link)
        {
            if (link == null)
                throw new ArgumentNullException(nameof(link));
            return new RuleLinkPair(link.Target, link);
        }
    }
}
