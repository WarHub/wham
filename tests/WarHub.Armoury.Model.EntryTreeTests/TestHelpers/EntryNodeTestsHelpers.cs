// WarHub licenses this file to you under the MIT license.
// See LICENSE file in the project root for more information.

namespace WarHub.Armoury.Model.EntryTreeTests.TestHelpers
{
    using System;
    using EntryTree;
    using NSubstitute;

    public static class EntryNodeTestsHelpers
    {
        public static EntryNode CreateNodeForEntry() => CreateNodeForEntry(Guid.NewGuid());

        public static EntryNode CreateNodeForEntry(Guid entryGuid)
        {
            var entrySub = Substitute.For<IEntry>();
            entrySub.Id.Value.Returns(entryGuid);
            return CreateNodeForEntry(entrySub);
        }

        public static EntryNode CreateNodeForEntry(IEntry entry)
        {
            return EntryNode.Create(EntryLinkPair.From(entry), Substitute.For<INode>());
        }

        public static EntryNode CreateNodeForEntryLink() => CreateNodeForEntryLink(Guid.NewGuid(), Guid.NewGuid());

        public static EntryNode CreateNodeForEntryLink(Guid entryGuid, Guid linkGuid)
        {
            var linkEntry = Substitute.For<IEntry>();
            linkEntry.Id.Value.Returns(entryGuid);
            var linkSub = Substitute.For<IEntryLink>();
            linkSub.Id.Value.Returns(linkGuid);
            linkSub.Target.Returns(linkEntry);
            linkSub.TargetId.Value.Returns(entryGuid);
            return CreateNodeForEntryLink(linkSub);
        }

        public static EntryNode CreateNodeForEntryLink(IEntryLink link)
        {
            return EntryNode.Create(EntryLinkPair.From(link), Substitute.For<INode>());
        }
    }
}
