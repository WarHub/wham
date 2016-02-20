// WarHub licenses this file to you under the MIT license.
// See LICENSE file in the project root for more information.

namespace WarHub.Armoury.Model
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    ///     Extensions for <see cref="IEntry" /> to check various preset predicates.
    /// </summary>
    public static class EntryExtensions
    {
        /// <summary>
        ///     Concatenates subentries with those from link collection in form of entry-optionalLink collection.
        /// </summary>
        /// <param name="catalogue">Source of entries and entry links.</param>
        /// <returns>Concatenated enumerable.</returns>
        public static IEnumerable<RootEntryLinkPair> GetEntryLinkPairs(this ICatalogue catalogue)
        {
            if (catalogue == null)
                throw new ArgumentNullException(nameof(catalogue));
            return
                catalogue.Entries.Select(RootEntryLinkPair.From)
                    .Concat(catalogue.EntryLinks.Select(RootEntryLinkPair.From));
        }

        /// <summary>
        ///     Concatenates subentries with those from link collection in form of entry-optionalLink collection.
        /// </summary>
        /// <param name="entryBase">Source of entries and entry links.</param>
        /// <returns>Concatenated enumerable.</returns>
        public static IEnumerable<EntryLinkPair> GetEntryLinkPairs(this IEntryBase entryBase)
        {
            if (entryBase == null)
                throw new ArgumentNullException(nameof(entryBase));
            return entryBase.Entries.Select(EntryLinkPair.From).Concat(entryBase.EntryLinks.Select(EntryLinkPair.From));
        }

        /// <summary>
        ///     Concatenates subgroups with those from link collection in form of group-optionalLink collection.
        /// </summary>
        /// <param name="entryBase">Source of entries and entry links.</param>
        /// <returns>Concatenated enumerable.</returns>
        public static IEnumerable<GroupLinkPair> GetGroupLinkPairs(this IEntryBase entryBase)
        {
            if (entryBase == null)
                throw new ArgumentNullException(nameof(entryBase));
            return entryBase.Groups.Select(GroupLinkPair.From).Concat(entryBase.GroupLinks.Select(GroupLinkPair.From));
        }

        /// <summary>
        ///     Concatenates profiles with those from link collection in form of profile-optionalLink collection.
        /// </summary>
        /// <param name="entry">Source of profiles and profile links.</param>
        /// <returns>Concatenated enumerable.</returns>
        public static IEnumerable<ProfileLinkPair> GetProfileLinkPairs(this IEntry entry)
        {
            if (entry == null)
                throw new ArgumentNullException(nameof(entry));
            return entry.Profiles.Select(ProfileLinkPair.From).Concat(entry.ProfileLinks.Select(ProfileLinkPair.From));
        }

        /// <summary>
        ///     Concatenates rules with those from link collection in form of rule-optionalLink collection.
        /// </summary>
        /// <param name="entry">Source of rules and rule links.</param>
        /// <returns>Concatenated enumerable.</returns>
        public static IEnumerable<RuleLinkPair> GetRuleLinkPairs(this IEntry entry)
        {
            if (entry == null)
                throw new ArgumentNullException(nameof(entry));
            return entry.Rules.Select(RuleLinkPair.From).Concat(entry.RuleLinks.Select(RuleLinkPair.From));
        }

        /// <summary>
        ///     Concatenates subentries and subentry links of the entry.
        /// </summary>
        /// <param name="entryBase">The entry to get subentries of.</param>
        /// <returns>Concatenated collection of subentries.</returns>
        public static IEnumerable<IEntry> GetSubEntries(this IEntryBase entryBase)
        {
            return entryBase.Entries.Concat(entryBase.EntryLinks.Select(el => el.Target));
        }

        /// <summary>
        ///     Concatenates subgroups and subgroup links of the entry.
        /// </summary>
        /// <param name="entryBase">The entry to get subgroups of.</param>
        /// <returns>Concatenated collection of subgroups.</returns>
        public static IEnumerable<IGroup> GetSubGroups(this IEntryBase entryBase)
        {
            return entryBase.Groups.Concat(entryBase.GroupLinks.Select(gl => gl.Target));
        }

        /// <summary>
        ///     Checks whether any child entry/group can be modded (it's count changed).
        /// </summary>
        /// <param name="entryBase">Checked entry.</param>
        /// <returns>True if they can, false otherwise.</returns>
        public static bool HasModdableContent(this IEntryBase entryBase)
        {
            return entryBase.GetSubEntries().Any(se => !se.IsStaticCount() || se.HasModdableContent()) ||
                   entryBase.GetSubGroups().Any(sg => !sg.IsStaticCount() || sg.HasModdableContent());
        }

        /// <summary>
        ///     Checks whether all sub entries and sub groups are collective and their subitems are too (recursively).
        /// </summary>
        /// <param name="entryBase">Entry to check subentries and subgroups of.</param>
        /// <returns>True if they are, false otherwise.</returns>
        public static bool IsAllContentCollective(this IEntryBase entryBase)
        {
            return entryBase.GetSubEntries().All(entry => entry.IsCollective && entry.IsAllContentCollective()) &&
                   entryBase.GetSubGroups().All(@group => group.IsCollective && group.IsAllContentCollective());
        }

        /// <summary>
        ///     Check if MinMax (selections limit) has different min and max values.
        /// </summary>
        /// <param name="entryBase"></param>
        /// <returns>True if min and max have the same value, false otherwise.</returns>
        public static bool IsStaticCount(this IEntryBase entryBase)
        {
            return entryBase.Limits.SelectionsLimit.MinEqualsMax();
        }
    }
}
