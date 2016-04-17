// WarHub licenses this file to you under the MIT license.
// See LICENSE file in the project root for more information.

namespace WarHub.Armoury.Model.EntryTreeTests.TestHelpers
{
    using System;
    using System.Linq;
    using NSubstitute;

    public static class EntryTestHelpers
    {
        public static IEntry CreateEntryWithChildren()
        {
            var subentries = Enumerable.Repeat<Func<IEntry>>(() =>
            {
                var entrySub = Substitute.For<IEntry>();
                entrySub.Id.Value.Returns(Guid.NewGuid());
                return entrySub;
            }, 3).Select(func => func()).ToList();
            var subgroups = Enumerable.Repeat<Func<IGroup>>(() =>
            {
                var groupSub = Substitute.For<IGroup>();
                groupSub.Id.Value.Returns(Guid.NewGuid());
                return groupSub;
            }, 3).Select(func => func()).ToList();

            var entryId = Guid.NewGuid();
            var entry = Substitute.For<IEntry>();
            entry.Id.Value.Returns(entryId);
            entry.Entries.Returns(new ReadonlyNodeSimple<IEntry>(subentries));
            entry.Groups.Returns(new ReadonlyNodeSimple<IGroup>(subgroups));
            return entry;
        }

        public static IEntryLink CreateLinkWithGuid(Guid linkId)
        {
            var entry = CreateEntryWithChildren();
            var entryId = entry.Id.Value;

            var link = Substitute.For<IEntryLink>();
            link.Id.Value.Returns(linkId);
            link.TargetId.Value.Returns(entryId);
            link.Target.Returns(entry);
            return link;
        }
    }
}
