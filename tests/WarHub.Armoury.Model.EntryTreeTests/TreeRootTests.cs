// WarHub licenses this file to you under the MIT license.
// See LICENSE file in the project root for more information.

namespace WarHub.Armoury.Model.EntryTreeTests
{
    using System;
    using System.Linq;
    using EntryTree;
    using NSubstitute;
    using TestHelpers;
    using Xunit;
    using static TestHelpers.EntryTestHelpers;

    public class TreeRootTests
    {
        [Fact]
        public void Create_FromCatalogue_Arg1Null_Fail()
        {
            Assert.Throws<ArgumentNullException>(() => { TreeRoot.Create(null, Substitute.For<ICategory>()); });
        }

        [Fact]
        public void Create_FromCatalogue_Arg2Null_Fail()
        {
            Assert.Throws<ArgumentNullException>(() => { TreeRoot.Create(Substitute.For<ICatalogue>(), null); });
        }

        [Fact]
        public void Create_FromCatalogue_Success()
        {
            var categoryId = Guid.NewGuid();
            var category = Substitute.For<ICategory>();
            category.Id.Value.Returns(categoryId);

            var entry = Substitute.For<IRootEntry>();
            entry.CategoryLink.Target.Returns(category);
            entry.CategoryLink.TargetId.Value.Returns(categoryId);

            var catalogue = Substitute.For<ICatalogue>();
            catalogue.Entries.Returns(new ReadonlyNodeSimple<IRootEntry> {entry});

            var root = TreeRoot.Create(catalogue, category);

            Assert.Same(entry, root.EntryNodes.Single().Entry);
            Assert.Equal(0, root.GroupNodes.Count());
        }

        [Fact]
        public void Create_FromEntry_ArgNull_Fail()
        {
            Assert.Throws<ArgumentNullException>(() => { TreeRoot.Create(null); });
        }

        [Fact]
        public void Ctor_FromCatalogue_Arg1Null_Fail()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                // ReSharper disable once ObjectCreationAsStatement
                new TreeRoot(null, Substitute.For<ICategory>());
            });
        }

        [Fact]
        public void Ctor_FromCatalogue_Arg2Null_Fail()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                // ReSharper disable once ObjectCreationAsStatement
                new TreeRoot(Substitute.For<ICatalogue>(), null);
            });
        }

        [Fact]
        public void Ctor_FromCatalogue_Success()
        {
            var categoryId = Guid.NewGuid();
            var category = Substitute.For<ICategory>();
            category.Id.Value.Returns(categoryId);

            var entry = Substitute.For<IRootEntry>();
            entry.CategoryLink.Target.Returns(category);
            entry.CategoryLink.TargetId.Value.Returns(categoryId);

            var catalogue = Substitute.For<ICatalogue>();
            catalogue.Entries.Returns(new ReadonlyNodeSimple<IRootEntry> {entry});

            var root = new TreeRoot(catalogue, category);

            Assert.Same(entry, root.EntryNodes.Single().Entry);
            Assert.Equal(0, root.GroupNodes.Count());
        }

        [Fact]
        public void Ctor_FromEntry_ArgNull_Fail()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                // ReSharper disable once ObjectCreationAsStatement
                new TreeRoot(null);
            });
        }

        [Fact]
        public void IsForLinkGuid_RandomValue_False_Success()
        {
            var node = CreateAsINode();

            var isForLinkResult = node.IsForLinkGuid(Guid.NewGuid());

            Assert.False(isForLinkResult);
        }

        [Fact]
        public void IsForLinkGuid_SameIdAsEntry_False_Success()
        {
            var entryId = Guid.NewGuid();
            var entry = Substitute.For<IEntry>();
            entry.Id.Value.Returns(entryId);
            var node = (INode) CreateWithEntry(entry);

            var isForLinkResult = node.IsForLinkGuid(entryId);

            Assert.False(isForLinkResult);
        }

        [Fact]
        public void PropertyAsEntryNode_Fail()
        {
            var node = CreateAsINode();

            Assert.Throws<NotSupportedException>(() =>
            {
                // ReSharper disable once UnusedVariable
                var entryNode = node.AsEntryNode;
            });
        }

        [Fact]
        public void PropertyAsGroupNode_Fail()
        {
            var node = CreateAsINode();

            Assert.Throws<NotSupportedException>(() =>
            {
                // ReSharper disable once UnusedVariable
                var entryNode = node.AsGroupNode;
            });
        }

        [Fact]
        public void PropertyChildren_NonEmpty_Success()
        {
            var entry = CreateEntryWithChildren();
            var node = CreateWithEntry(entry);
            var expectedCount = entry.GetEntryLinkPairs().Count() + entry.GetGroupLinkPairs().Count();

            var actualCount = node.Children.Count();

            Assert.Equal(expectedCount, actualCount);
        }

        [Fact]
        public void PropertyEntryNodes_NonEmpty_Success()
        {
            var entry = CreateEntryWithChildren();
            var node = CreateWithEntry(entry);
            var expectedCount = entry.GetEntryLinkPairs().Count();

            var actualCount = node.EntryNodes.Count();

            Assert.Equal(expectedCount, actualCount);
        }

        [Fact]
        public void PropertyGroupNodes_NotEmpty_Success()
        {
            var entry = CreateEntryWithChildren();
            var node = CreateWithEntry(entry);
            var expectedCount = entry.GetEntryLinkPairs().Count();

            var actualCount = node.GroupNodes.Count();

            Assert.Equal(expectedCount, actualCount);
        }

        [Fact]
        public void PropertyIsEntryNode_False_Success()
        {
            var node = CreateAsINode();

            var isEntryNode = node.IsEntryNode;

            Assert.False(isEntryNode);
        }

        [Fact]
        public void PropertyIsGroupNode_False_Success()
        {
            var node = CreateAsINode();

            var isGroupNode = node.IsGroupNode;

            Assert.False(isGroupNode);
        }

        [Fact]
        public void PropertyIsLinkNode_False_Success()
        {
            var node = CreateAsINode();

            var isLinkNode = node.IsLinkNode;

            Assert.False(isLinkNode);
        }

        [Fact]
        public void PropertyIsRoot_True_Success()
        {
            var node = CreateAsINode();

            var isRoot = node.IsRoot;

            Assert.True(isRoot);
        }

        [Fact]
        public void PropertyParent_ReturnsSelf_Success()
        {
            var node = CreateAsINode();

            var parent = node.Parent;

            Assert.Same(node, parent);
        }


        private static TreeRoot Create()
        {
            return CreateWithEntry(Substitute.For<IEntry>());
        }

        private static INode CreateAsINode()
        {
            return Create();
        }

        private static TreeRoot CreateWithEntry(IEntry entry)
        {
            return new TreeRoot(entry);
        }
    }
}
