// WarHub licenses this file to you under the MIT license.
// See LICENSE file in the project root for more information.

namespace WarHub.Armoury.Model.Builders.Implementations
{
    using System;

    internal static class LinkPairGuidExtensions
    {
        public static bool AnyHasId(this EntryLinkPair pair, Guid guid)
        {
            return pair.Entry.IdValueEquals(guid) || pair.HasLink && pair.Link.IdValueEquals(guid);
        }

        public static bool AnyHasId(this GroupLinkPair pair, Guid guid)
        {
            return pair.Group.IdValueEquals(guid) || pair.HasLink && pair.Link.IdValueEquals(guid);
        }

        public static bool AnyHasId(this ProfileLinkPair pair, Guid guid)
        {
            return pair.Profile.IdValueEquals(guid) || pair.HasLink && pair.Link.IdValueEquals(guid);
        }

        public static bool AnyHasId(this RuleLinkPair pair, Guid guid)
        {
            return pair.Rule.IdValueEquals(guid) || pair.HasLink && pair.Link.IdValueEquals(guid);
        }
    }
}
