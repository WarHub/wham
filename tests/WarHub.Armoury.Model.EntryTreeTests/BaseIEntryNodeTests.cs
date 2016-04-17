// WarHub licenses this file to you under the MIT license.
// See LICENSE file in the project root for more information.

namespace WarHub.Armoury.Model.EntryTreeTests
{
    using System;
    using System.Linq;
    using EntryTree;
    using NSubstitute;
    using Xunit;
    using static TestHelpers.EntryTestHelpers;

    /// <summary>
    ///     Unit tests which should be passed by any implementation of <see cref="IEntryNode" />.
    /// </summary>
    public abstract class BaseIEntryNodeTests
    {
        [Fact]
        public void IsForLinkGuid_DifferentValue_False_Success()
        {
            var linkId = Guid.NewGuid();
            var link = CreateLinkWithGuid(linkId);
            var node = CreateEntryNodeFromLink(link);

            var isForLinkResult = node.IsForLinkGuid(Guid.NewGuid());

            Assert.False(isForLinkResult);
        }

        [Fact]
        public void IsForLinkGuid_NoLink_False_Success()
        {
            var entry = CreateEntryWithChildren();
            var entryId = entry.Id.Value;
            var node = CreateEntryNodeFromEntry(entry);

            var isForLinkResult = node.IsForLinkGuid(entryId);

            Assert.False(isForLinkResult);
        }

        [Fact]
        public void IsForLinkGuid_SameValue_True_Success()
        {
            var linkId = Guid.NewGuid();
            var link = CreateLinkWithGuid(linkId);
            var node = CreateEntryNodeFromLink(link);

            var isForLinkResult = node.IsForLinkGuid(linkId);

            Assert.True(isForLinkResult);
        }

        [Fact]
        public void PropertyAsEntryNode_ReturnSelf_Success()
        {
            var node = (INode) CreateEntryNodeFromEntry();

            var entryNode = node.AsEntryNode;

            Assert.Same(node, entryNode);
        }

        [Fact]
        public void PropertyAsGroupNode_Fail()
        {
            var node = (INode) CreateEntryNodeFromEntry();

            Assert.Throws<NotSupportedException>(() =>
            {
                // ReSharper disable once UnusedVariable
                var groupNode = node.AsGroupNode;
            });
        }

        [Fact]
        public void PropertyChildren_Empty_Success()
        {
            var entry = CreateEntryWithChildren();
            var node = CreateEntryNodeFromEntry(entry);

            var actualCount = node.Children.Count();

            Assert.Equal(0, actualCount);
        }

        [Fact]
        public void PropertyEntry_EntryInitialized_Success()
        {
            var entry = Substitute.For<IEntry>();
            var node = CreateEntryNodeFromEntry(entry);

            var retrievedEntry = node.Entry;

            Assert.Same(entry, retrievedEntry);
        }

        [Fact]
        public void PropertyEntry_LinkInitialized_Success()
        {
            var link = Substitute.For<IEntryLink>();
            var entry = link.Target;
            var node = CreateEntryNodeFromLink(link);

            var retrievedEntry = node.Entry;

            Assert.Same(entry, retrievedEntry);
        }

        [Fact]
        public void PropertyEntryLinkPair_EntryInitialized_Success()
        {
            var node = CreateEntryNodeFromEntry();

            var retrievedPair = node.EntryLinkPair;

            Assert.Same(node.Entry, retrievedPair.Entry);
            Assert.Null(retrievedPair.Link);
        }

        [Fact]
        public void PropertyEntryLinkPair_LinkInitialized_Success()
        {
            var node = CreateEntryNodeFromLink();

            var retrievedPair = node.EntryLinkPair;

            Assert.Same(node.Entry, retrievedPair.Entry);
            Assert.Same(node.Link, retrievedPair.Link);
        }

        [Fact]
        public void PropertyEntryNodes_Empty_Success()
        {
            var entry = CreateEntryWithChildren();
            var node = CreateEntryNodeFromEntry(entry);

            var actualCount = node.EntryNodes.Count();

            Assert.Equal(0, actualCount);
        }

        [Fact]
        public void PropertyGroupNodes_Empty_Success()
        {
            var entry = CreateEntryWithChildren();
            var node = CreateEntryNodeFromEntry(entry);

            var actualCount = node.GroupNodes.Count();

            Assert.Equal(0, actualCount);
        }

        [Fact]
        public void PropertyIsEntryNode_True_Success()
        {
            var node = CreateEntryNodeFromEntry();

            var isEntryNode = node.IsEntryNode;

            Assert.True(isEntryNode);
        }

        [Fact]
        public void PropertyIsGroupNode_False_Success()
        {
            var node = CreateEntryNodeFromEntry();

            var isGroupNode = node.IsGroupNode;

            Assert.False(isGroupNode);
        }

        [Fact]
        public void PropertyIsLinkNode_False_Success()
        {
            var node = CreateEntryNodeFromEntry();

            var isLinkNode = node.IsLinkNode;

            Assert.False(isLinkNode);
        }

        [Fact]
        public void PropertyIsLinkNode_True_Success()
        {
            var node = CreateEntryNodeFromLink();

            var isLinkNode = node.IsLinkNode;

            Assert.True(isLinkNode);
        }

        [Fact]
        public void PropertyIsRoot_False_Success()
        {
            var node = CreateEntryNodeFromEntry();

            var isRoot = node.IsRoot;

            Assert.False(isRoot);
        }

        [Fact]
        public void PropertyLink_Initialized_Success()
        {
            var link = Substitute.For<IEntryLink>();
            var node = CreateEntryNodeFromLink(link);

            var retrievedLink = node.Link;

            Assert.Same(link, retrievedLink);
        }

        [Fact]
        public void PropertyLink_NullInitialized_Success()
        {
            var node = CreateEntryNodeFromEntry();

            var retrievedLink = node.Link;

            Assert.Null(retrievedLink);
        }

        [Fact]
        public void PropertyParent_NotNull_Success()
        {
            var node = CreateEntryNodeFromEntry();

            var parent = node.Parent;

            Assert.NotNull(parent);
        }

        protected abstract IEntryNode CreateEntryNodeFromEntry(IEntry entry);
        protected abstract IEntryNode CreateEntryNodeFromEntry();
        protected abstract IEntryNode CreateEntryNodeFromLink(IEntryLink link);
        protected abstract IEntryNode CreateEntryNodeFromLink();
    }
}
