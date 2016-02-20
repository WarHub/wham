// WarHub licenses this file to you under the MIT license.
// See LICENSE file in the project root for more information.

namespace WarHub.Armoury.Model
{
    using System;

    public static class IdentifiableExtensions
    {
        public static bool IdValueEquals(this IIdentifiable identifiable, Guid guid)
        {
            if (identifiable == null)
                throw new ArgumentNullException(nameof(identifiable));
            return identifiable.Id.Value == guid;
        }

        public static bool LinksTo(this ILink link, IIdentifiable target)
        {
            if (link == null)
                throw new ArgumentNullException(nameof(link));
            if (target == null)
                throw new ArgumentNullException(nameof(target));
            return link.TargetId.Value == target.Id.Value;
        }

        public static bool TargetIdValueEquals(this ILink link, Guid otherGuid)
        {
            if (link == null)
                throw new ArgumentNullException(nameof(link));
            return link.TargetId.Value == otherGuid;
        }

        public static bool TargetIdValuesAreEqual(this ILink link, ILink other)
        {
            if (link == null)
                throw new ArgumentNullException(nameof(link));
            if (other == null)
                throw new ArgumentNullException(nameof(other));
            return link.TargetId.Value == other.TargetId.Value;
        }
    }
}
