// WarHub licenses this file to you under the MIT license.
// See LICENSE file in the project root for more information.

namespace WarHub.Armoury.Model
{
    using System;

    /// <summary>
    ///     Represents entry with optional link which pointed to it in source node.
    /// </summary>
    public class EntryLinkPair
    {
        protected EntryLinkPair(IEntry entry, IEntryLink link)
        {
            if (entry == null)
                throw new ArgumentNullException(nameof(entry));
            Entry = entry;
            Link = link;
        }

        /// <summary>
        ///     Gets entry of this pair. Cannot be null.
        /// </summary>
        public IEntry Entry { get; }

        /// <summary>
        ///     Gets if this pair contains <see cref="Link" />.
        /// </summary>
        public bool HasLink => Link != null;

        /// <summary>
        ///     Gets optional link which targets <see cref="Entry" />. May be null.
        /// </summary>
        public IEntryLink Link { get; }

        public static EntryLinkPair From(IEntry entry)
        {
            return new EntryLinkPair(entry, null);
        }

        public static EntryLinkPair From(IEntryLink link)
        {
            if (link == null)
                throw new ArgumentNullException(nameof(link));
            return new EntryLinkPair(link.Target, link);
        }
    }
}
