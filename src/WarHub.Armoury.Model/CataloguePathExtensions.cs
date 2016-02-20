// WarHub licenses this file to you under the MIT license.
// See LICENSE file in the project root for more information.

namespace WarHub.Armoury.Model
{
    using System;

    public static class CataloguePathExtensions
    {
        /// <summary>
        ///     Depending on whether <paramref name="link" /> is null or not,
        ///     <see cref="CataloguePath.Select(IIdentifiable)" /> or
        ///     <see cref="CataloguePath.Select(IEntryLink)" /> is invoked.
        /// </summary>
        /// <param name="path">Base path.</param>
        /// <param name="entry">Entry to (optionally) be 'Select'ed.</param>
        /// <param name="link">Link to (optionally) be 'Select'ed.</param>
        /// <returns>Result of appropriate 'Select' invocation.</returns>
        public static CataloguePath SelectAuto(this CataloguePath path, IEntry entry, IEntryLink link)
        {
            if (path == null)
                throw new ArgumentNullException(nameof(path));
            if (entry == null)
                throw new ArgumentNullException(nameof(entry));
            return link == null ? path.Select(entry) : path.Select(link);
        }

        /// <summary>
        ///     Depending on whether <paramref name="entryPair" />'s <see cref="EntryLinkPair.Link" /> is null or
        ///     not, <see cref="CataloguePath.Select(IIdentifiable)" /> or <see cref="CataloguePath.Select(IEntryLink)" /> is
        ///     invoked.
        /// </summary>
        /// <param name="path">Base path.</param>
        /// <param name="entryPair">Entry-link pair to select one of it's values.</param>
        /// <returns>Result of appropriate 'Select' invocation.</returns>
        public static CataloguePath SelectAuto(this CataloguePath path, EntryLinkPair entryPair)
        {
            if (path == null)
                throw new ArgumentNullException(nameof(path));
            if (entryPair == null)
                throw new ArgumentNullException(nameof(entryPair));
            return entryPair.Link == null ? path.Select(entryPair.Entry) : path.Select(entryPair.Link);
        }

        /// <summary>
        ///     Depending on whether <paramref name="link" /> is null or not,
        ///     <see cref="CataloguePath.Select(IIdentifiable)" /> or
        ///     <see cref="CataloguePath.Select(IGroupLink)" /> is invoked.
        /// </summary>
        /// <param name="path">Base path.</param>
        /// <param name="group">Group to (optionally) be 'Select'ed.</param>
        /// <param name="link">Link to (optionally) be 'Select'ed.</param>
        /// <returns>Result of 'Select' invocation.</returns>
        public static CataloguePath SelectAuto(this CataloguePath path, IGroup @group, IGroupLink link)
        {
            if (path == null)
                throw new ArgumentNullException(nameof(path));
            if (@group == null)
                throw new ArgumentNullException(nameof(@group));
            return link == null ? path.Select(@group) : path.Select(link);
        }

        /// <summary>
        ///     Depending on whether <paramref name="groupPair" />'s <see cref="GroupLinkPair.Link" /> is null or
        ///     not, <see cref="CataloguePath.Select(IIdentifiable)" /> or <see cref="CataloguePath.Select(IGroupLink)" /> is
        ///     invoked.
        /// </summary>
        /// <param name="path">Base path.</param>
        /// <param name="groupPair">Group-link pair to select one of it's values.</param>
        /// <returns>Result of appropriate 'Select' invocation.</returns>
        public static CataloguePath SelectAuto(this CataloguePath path, GroupLinkPair groupPair)
        {
            if (path == null)
                throw new ArgumentNullException(nameof(path));
            if (groupPair == null)
                throw new ArgumentNullException(nameof(groupPair));
            return groupPair.Link == null ? path.Select(groupPair.Group) : path.Select(groupPair.Link);
        }

        /// <summary>
        ///     Depending on whether <paramref name="link" /> is null or not,
        ///     <see cref="CataloguePath.Select(IIdentifiable)" /> or
        ///     <see cref="CataloguePath.Select(IProfileLink)" /> is invoked.
        /// </summary>
        /// <param name="path">Base path.</param>
        /// <param name="profile">Profile to (optionally) be 'Select'ed.</param>
        /// <param name="link">Link to (optionally) be 'Select'ed.</param>
        /// <returns>Result of 'Select' invocation.</returns>
        public static CataloguePath SelectAuto(this CataloguePath path, IProfile profile, IProfileLink link)
        {
            if (path == null)
                throw new ArgumentNullException(nameof(path));
            if (profile == null)
                throw new ArgumentNullException(nameof(profile));
            return link == null ? path.Select(profile) : path.Select(link);
        }

        /// <summary>
        ///     Depending on whether <paramref name="link" /> is null or not,
        ///     <see cref="CataloguePath.Select(IIdentifiable)" /> or
        ///     <see cref="CataloguePath.Select(IRuleLink)" /> is invoked.
        /// </summary>
        /// <param name="path">Base path.</param>
        /// <param name="rule">Rule to (optionally) be 'Select'ed.</param>
        /// <param name="link">Link to (optionally) be 'Select'ed.</param>
        /// <returns>Result of 'Select' invocation.</returns>
        public static CataloguePath SelectAuto(this CataloguePath path, IRule rule, IRuleLink link)
        {
            if (path == null)
                throw new ArgumentNullException(nameof(path));
            if (rule == null)
                throw new ArgumentNullException(nameof(rule));
            return link == null ? path.Select(rule) : path.Select(link);
        }
    }
}
