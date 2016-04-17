// WarHub licenses this file to you under the MIT license.
// See LICENSE file in the project root for more information.

namespace WarHub.Armoury.Model.EntryTreeTests.TestHelpers
{
    using System;
    using EntryTree;
    using NSubstitute;

    public static class GroupNodeTestsHelpers
    {
        public static GroupNode CreateNodeForGroup() => CreateNodeForGroup(Guid.NewGuid());

        public static GroupNode CreateNodeForGroup(Guid groupGuid)
        {
            var groupSub = Substitute.For<IGroup>();
            groupSub.Id.Value.Returns(groupGuid);
            return CreateNodeForGroup(groupSub);
        }

        public static GroupNode CreateNodeForGroup(IGroup group)
        {
            return GroupNode.Create(GroupLinkPair.From(group), Substitute.For<INode>());
        }

        public static GroupNode CreateNodeForGroupLink(IGroupLink link)
        {
            return GroupNode.Create(GroupLinkPair.From(link), Substitute.For<INode>());
        }

        public static GroupNode CreateNodeForGroupLink() => CreateNodeForGroupLink(Guid.NewGuid(), Guid.NewGuid());

        public static GroupNode CreateNodeForGroupLink(Guid groupGuid, Guid linkGuid)
        {
            var linkGroup = Substitute.For<IGroup>();
            linkGroup.Id.Value.Returns(groupGuid);
            var linkSub = Substitute.For<IGroupLink>();
            linkSub.Id.Value.Returns(linkGuid);
            linkSub.Target.Returns(linkGroup);
            linkSub.TargetId.Value.Returns(groupGuid);
            return CreateNodeForGroupLink(linkSub);
        }
    }
}
