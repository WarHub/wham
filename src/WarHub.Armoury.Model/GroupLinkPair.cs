// WarHub licenses this file to you under the MIT license.
// See LICENSE file in the project root for more information.

namespace WarHub.Armoury.Model
{
    using System;

    /// <summary>
    ///     Represents group with optional link which pointed to it in source node.
    /// </summary>
    public class GroupLinkPair
    {
        private GroupLinkPair(IGroup @group, IGroupLink link)
        {
            if (@group == null)
                throw new ArgumentNullException(nameof(@group));
            Group = @group;
            Link = link;
        }

        /// <summary>
        ///     Gets group of this pair. Cannot be null.
        /// </summary>
        public IGroup Group { get; }

        /// <summary>
        ///     Gets if this pair contains <see cref="Link" />.
        /// </summary>
        public bool HasLink => Link != null;

        /// <summary>
        ///     Gets optional link which targets <see cref="Group" />. May be null.
        /// </summary>
        public IGroupLink Link { get; }

        public static GroupLinkPair From(IGroup group)
        {
            return new GroupLinkPair(group, null);
        }

        public static GroupLinkPair From(IGroupLink link)
        {
            if (link == null)
                throw new ArgumentNullException(nameof(link));
            return new GroupLinkPair(link.Target, link);
        }
    }
}
