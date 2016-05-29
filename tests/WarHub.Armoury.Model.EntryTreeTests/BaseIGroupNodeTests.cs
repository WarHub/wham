// WarHub licenses this file to you under the MIT license.
// See LICENSE file in the project root for more information.

namespace WarHub.Armoury.Model.EntryTreeTests
{
    using System;
    using System.Linq;
    using EntryTree;
    using NSubstitute;
    using Xunit;

    /// <summary>
    ///     Unit tests which should be passed by any implementation of <see cref="IGroupNode" />.
    /// </summary>
    public abstract class BaseIGroupNodeTests
    {
        [Fact]
        public void IsForLinkGuid_DifferentValue_False_Success()
        {
            var linkId = Guid.NewGuid();
            var link = CreateLinkWithGuid(linkId);
            var node = CreateGroupNodeFromLink(link);

            var isForLinkResult = node.IsForLinkGuid(Guid.NewGuid());

            Assert.False(isForLinkResult);
        }

        [Fact]
        public void IsForLinkGuid_NoLink_False_Success()
        {
            var group = CreateGroupWithChildren();
            var groupId = group.Id.Value;
            var node = CreateGroupNodeFromGroup(group);

            var isForLinkResult = node.IsForLinkGuid(groupId);

            Assert.False(isForLinkResult);
        }

        [Fact]
        public void IsForLinkGuid_SameValue_True_Success()
        {
            var linkId = Guid.NewGuid();
            var link = CreateLinkWithGuid(linkId);
            var node = CreateGroupNodeFromLink(link);

            var isForLinkResult = node.IsForLinkGuid(linkId);

            Assert.True(isForLinkResult);
        }

        [Fact]
        public void PropertyAsEntryNode_Fail()
        {
            var node = (INode) CreateGroupNodeFromGroup();

            Assert.Throws<NotSupportedException>(() =>
            {
                // ReSharper disable once UnusedVariable
                var x = node.AsEntryNode;
            });
        }

        [Fact]
        public void PropertyAsGroupNode_ReturnSelf_Success()
        {
            var node = (INode) CreateGroupNodeFromGroup();

            var groupNode = node.AsGroupNode;

            Assert.Same(node, groupNode);
        }

        [Fact]
        public void PropertyChildren_NonEmpty_Success()
        {
            var group = CreateGroupWithChildren();
            var node = CreateGroupNodeFromGroup(group);
            var expectedCount = group.GetEntryLinkPairs().Count() + group.GetGroupLinkPairs().Count();

            var actualCount = node.Children.Count();

            Assert.Equal(expectedCount, actualCount);
        }

        [Fact]
        public void PropertyEntryNodes_NonEmpty_Success()
        {
            var group = CreateGroupWithChildren();
            var node = CreateGroupNodeFromGroup(group);
            var expectedCount = group.GetEntryLinkPairs().Count();

            var actualCount = node.EntryNodes.Count();

            Assert.Equal(expectedCount, actualCount);
        }

        [Fact]
        public void PropertyGroup_GroupInitialized_Success()
        {
            var entry = Substitute.For<IGroup>();
            var node = CreateGroupNodeFromGroup(entry);

            var retrievedGroup = node.Group;

            Assert.Same(entry, retrievedGroup);
        }

        [Fact]
        public void PropertyGroup_LinkInitialized_Success()
        {
            var link = Substitute.For<IGroupLink>();
            var group = link.Target;
            var node = CreateGroupNodeFromLink(link);

            var retrievedGroup = node.Group;

            Assert.Same(group, retrievedGroup);
        }

        [Fact]
        public void PropertyGroupLinkPair_GroupInitialized_Success()
        {
            var node = CreateGroupNodeFromGroup();

            var retrievedPair = node.GroupLinkPair;

            Assert.Same(node.Group, retrievedPair.Group);
            Assert.Null(retrievedPair.Link);
        }

        [Fact]
        public void PropertyGroupLinkPair_LinkInitialized_Success()
        {
            var node = CreateGroupNodeFromLink();

            var retrievedPair = node.GroupLinkPair;

            Assert.Same(node.Group, retrievedPair.Group);
            Assert.Same(node.Link, retrievedPair.Link);
        }

        [Fact]
        public void PropertyGroupNodes_NotEmpty_Success()
        {
            var group = CreateGroupWithChildren();
            var node = CreateGroupNodeFromGroup(group);
            var expectedCount = group.GetGroupLinkPairs().Count();

            var actualCount = node.GroupNodes.Count();

            Assert.Equal(expectedCount, actualCount);
        }

        [Fact]
        public void PropertyIsEntryNode_False_Success()
        {
            var node = CreateGroupNodeFromGroup();

            var isEntryNode = node.IsEntryNode;

            Assert.False(isEntryNode);
        }

        [Fact]
        public void PropertyIsGroupNode_True_Success()
        {
            var node = CreateGroupNodeFromGroup();

            var isGroupNode = node.IsGroupNode;

            Assert.True(isGroupNode);
        }

        [Fact]
        public void PropertyIsLinkNode_False_Success()
        {
            var node = CreateGroupNodeFromGroup();

            var isLinkNode = node.IsLinkNode;

            Assert.False(isLinkNode);
        }

        [Fact]
        public void PropertyIsLinkNode_True_Success()
        {
            var node = CreateGroupNodeFromLink();

            var isLinkNode = node.IsLinkNode;

            Assert.True(isLinkNode);
        }

        [Fact]
        public void PropertyIsRoot_False_Success()
        {
            var node = CreateGroupNodeFromGroup();

            var isRoot = node.IsRoot;

            Assert.False(isRoot);
        }

        [Fact]
        public void PropertyLink_Initialized_Success()
        {
            var link = Substitute.For<IGroupLink>();
            var node = CreateGroupNodeFromLink(link);

            var retrievedLink = node.Link;

            Assert.Same(link, retrievedLink);
        }

        [Fact]
        public void PropertyLink_NullInitialized_Success()
        {
            var node = CreateGroupNodeFromGroup();

            var retrievedLink = node.Link;

            Assert.Null(retrievedLink);
        }

        [Fact]
        public void PropertyParent_NotNull_Success()
        {
            var node = CreateGroupNodeFromGroup();

            var parent = node.Parent;

            Assert.NotNull(parent);
        }

        protected abstract IGroupNode CreateGroupNodeFromGroup(IGroup group);
        protected abstract IGroupNode CreateGroupNodeFromGroup();
        protected abstract IGroupNode CreateGroupNodeFromLink(IGroupLink link);
        protected abstract IGroupNode CreateGroupNodeFromLink();

        private static IGroup CreateGroupWithChildren()
        {
            var subentries =
                Enumerable.Repeat<Func<IEntry>>(() => Substitute.For<IEntry>(), 3).Select(func => func()).ToList();
            var subgroups =
                Enumerable.Repeat<Func<IGroup>>(() => Substitute.For<IGroup>(), 3).Select(func => func()).ToList();

            var entryId = Guid.NewGuid();
            var group = Substitute.For<IGroup>();
            group.Id.Value.Returns(entryId);
            group.Entries.GetEnumerator().Returns(_ => subentries.GetEnumerator());
            group.Entries.Count.Returns(subentries.Count);
            group.Groups.GetEnumerator().Returns(_ => subgroups.GetEnumerator());
            group.Groups.Count.Returns(subgroups.Count);
            return group;
        }

        private static IGroupLink CreateLinkWithGuid(Guid linkId)
        {
            var group = CreateGroupWithChildren();
            var groupId = group.Id.Value;

            var link = Substitute.For<IGroupLink>();
            link.Id.Value.Returns(linkId);
            link.TargetId.Value.Returns(groupId);
            link.Target.Returns(group);
            return link;
        }
    }
}
