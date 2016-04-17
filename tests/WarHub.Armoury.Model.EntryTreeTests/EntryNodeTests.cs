// WarHub licenses this file to you under the MIT license.
// See LICENSE file in the project root for more information.

namespace WarHub.Armoury.Model.EntryTreeTests
{
    using System;
    using EntryTree;
    using NSubstitute;
    using Xunit;
    using static TestHelpers.EntryNodeTestsHelpers;

    public class EntryNodeTests : BaseIEntryNodeTests
    {
        protected override IEntryNode CreateEntryNodeFromEntry(IEntry entry)
        {
            return CreateNodeForEntry(entry);
        }

        protected override IEntryNode CreateEntryNodeFromEntry()
        {
            return CreateNodeForEntry();
        }

        protected override IEntryNode CreateEntryNodeFromLink(IEntryLink link)
        {
            return CreateNodeForEntryLink(link);
        }

        protected override IEntryNode CreateEntryNodeFromLink()
        {
            return CreateNodeForEntryLink();
        }

        [Fact]
        public void Create_Arg1Null_Fail()
        {
            Assert.Throws<ArgumentNullException>(() => { EntryNode.Create(null, Substitute.For<INode>()); });
        }

        [Fact]
        public void Create_Arg2Null_Fail()
        {
            Assert.Throws<ArgumentNullException>(
                () => { EntryNode.Create(EntryLinkPair.From(Substitute.For<IEntry>()), null); });
        }

        [Fact]
        public void Ctor_Arg1Null_Fail()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                // ReSharper disable once ObjectCreationAsStatement
                new EntryNode(null, Substitute.For<INode>());
            });
        }

        [Fact]
        public void Ctor_Arg2Null_Fail()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                // ReSharper disable once ObjectCreationAsStatement
                new EntryNode(EntryLinkPair.From(Substitute.For<IEntry>()), null);
            });
        }
    }
}
