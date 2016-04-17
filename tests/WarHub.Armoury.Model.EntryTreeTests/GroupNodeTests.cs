// WarHub licenses this file to you under the MIT license.
// See LICENSE file in the project root for more information.

namespace WarHub.Armoury.Model.EntryTreeTests
{
    using System;
    using EntryTree;
    using NSubstitute;
    using Xunit;
    using static TestHelpers.GroupNodeTestsHelpers;

    public class GroupNodeTests : BaseIGroupNodeTests
    {
        protected override IGroupNode CreateGroupNodeFromGroup(IGroup group)
        {
            return CreateNodeForGroup(group);
        }

        protected override IGroupNode CreateGroupNodeFromGroup()
        {
            return CreateNodeForGroup();
        }

        protected override IGroupNode CreateGroupNodeFromLink(IGroupLink link)
        {
            return CreateNodeForGroupLink(link);
        }

        protected override IGroupNode CreateGroupNodeFromLink()
        {
            return CreateNodeForGroupLink();
        }

        [Fact]
        public void Create_Arg1Null_Fail()
        {
            Assert.Throws<ArgumentNullException>(() => { GroupNode.Create(null, Substitute.For<INode>()); });
        }

        [Fact]
        public void Create_Arg2Null_Fail()
        {
            Assert.Throws<ArgumentNullException>(
                () => { GroupNode.Create(GroupLinkPair.From(Substitute.For<IGroup>()), null); });
        }

        [Fact]
        public void Ctor_Arg1Null_Fail()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                // ReSharper disable once ObjectCreationAsStatement
                new GroupNode(null, Substitute.For<INode>());
            });
        }

        [Fact]
        public void Ctor_Arg2Null_Fail()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                // ReSharper disable once ObjectCreationAsStatement
                new GroupNode(GroupLinkPair.From(Substitute.For<IGroup>()), null);
            });
        }
    }
}
