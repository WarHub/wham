// WarHub licenses this file to you under the MIT license.
// See LICENSE file in the project root for more information.

namespace WarHub.Armoury.Model
{
    using System;

    public class RootEntryLinkPair : EntryLinkPair
    {
        private RootEntryLinkPair(IEntry entry, IRootEntry rootEntry, IRootLink rootLink) : base(entry, rootLink)
        {
            RootRootEntry = rootEntry;
            RootLink = rootLink;
            CategoryId = rootEntry != null ? rootEntry.CategoryLink.TargetId : rootLink.CategoryLink.TargetId;
        }

        public IIdentifier CategoryId { get; }

        public IRootLink RootLink { get; }

        public IRootEntry RootRootEntry { get; }

        public static RootEntryLinkPair From(IRootEntry rootEntry)
        {
            if (rootEntry == null)
                throw new ArgumentNullException(nameof(rootEntry));
            return new RootEntryLinkPair(rootEntry, rootEntry, null);
        }

        public static RootEntryLinkPair From(IRootLink rootLink)
        {
            if (rootLink == null)
                throw new ArgumentNullException(nameof(rootLink));
            return new RootEntryLinkPair(rootLink.Target, null, rootLink);
        }
    }
}
