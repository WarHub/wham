// WarHub licenses this file to you under the MIT license.
// See LICENSE file in the project root for more information.

namespace WarHub.Armoury.Model
{
    using System;

    /// <summary>
    ///     Represents profile with optional link which pointed to it in source node.
    /// </summary>
    public class ProfileLinkPair
    {
        private ProfileLinkPair(IProfile profile, IProfileLink link)
        {
            if (profile == null)
                throw new ArgumentNullException(nameof(profile));
            Profile = profile;
            Link = link;
        }

        /// <summary>
        ///     Gets if this pair contains <see cref="Link" />.
        /// </summary>
        public bool HasLink => Link != null;

        /// <summary>
        ///     Gets optional link which targets <see cref="Profile" />. May be null.
        /// </summary>
        public IProfileLink Link { get; }

        /// <summary>
        ///     Gets profile of this pair. Cannot be null.
        /// </summary>
        public IProfile Profile { get; }

        public static ProfileLinkPair From(IProfile profile)
        {
            return new ProfileLinkPair(profile, null);
        }

        public static ProfileLinkPair From(IProfileLink link)
        {
            if (link == null)
                throw new ArgumentNullException(nameof(link));
            return new ProfileLinkPair(link.Target, link);
        }
    }
}
