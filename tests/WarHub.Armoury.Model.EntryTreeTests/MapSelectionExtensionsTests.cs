// WarHub licenses this file to you under the MIT license.
// See LICENSE file in the project root for more information.

namespace WarHub.Armoury.Model.EntryTreeTests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using EntryTree;
    using NSubstitute;
    using TestHelpers;
    using Xunit;

    public class MapSelectionExtensionsTests
    {
        [Fact]
        public void MapSelections_Entry_OnlyLinkFitting_Fail()
        {
            var entry = CreateEntry();

            var expectedRoot = TreeRoot.Create(new SubBuilder
            {
                Entries = new List<IEntry>
                {
                    entry
                }
            }.BuildEntry());
            var actualRoot = TreeRoot.Create(new SubBuilder
            {
                EntryLinks = new List<IEntryLink>
                {
                    LinkTo(entry)
                }
            }.BuildEntry());

            var entryNode = FindNodeFor(entry, expectedRoot);

            var selection = CreateForEntryNode(entryNode);
            var parentSelection = CreateParentSelection(selection);

            Assert.Throws<InvalidOperationException>(() =>
            {
                // ReSharper disable once UnusedVariable
                var map = actualRoot.MapSelections(parentSelection);
            });
        }

        [Fact]
        public void MapSelections_Entry_Success()
        {
            var entry = CreateEntry();

            var root = TreeRoot.Create(new SubBuilder
            {
                Entries = new List<IEntry>
                {
                    entry
                }
            }.BuildEntry());

            var entryNode = FindNodeFor(entry, root);

            var selection = CreateForEntryNode(entryNode);
            var parentSelection = CreateParentSelection(selection);

            var map = root.MapSelections(parentSelection);

            Assert.True(map.ContainsKey(entryNode));
            Assert.Same(selection, map[entryNode].Single());
        }

        [Fact]
        public void MapSelections_EntryLink_Success()
        {
            var entry = CreateEntry();

            var root = TreeRoot.Create(new SubBuilder
            {
                EntryLinks = new List<IEntryLink>
                {
                    LinkTo(entry)
                }
            }.BuildEntry());

            var entryNode = FindNodeFor(entry, root);

            var selection = CreateForEntryNode(entryNode);
            var parentSelection = CreateParentSelection(selection);

            var map = root.MapSelections(parentSelection);

            Assert.True(map.ContainsKey(entryNode));
            Assert.Same(selection, map[entryNode].Single());
        }

        [Fact]
        public void MapSelections_GroupedEntry_Success()
        {
            var entry = CreateEntry();

            var root = TreeRoot.Create(new SubBuilder
            {
                Groups = new List<IGroup>
                {
                    new SubBuilder
                    {
                        Entries = new List<IEntry>
                        {
                            entry
                        }
                    }.BuildGroup()
                }
            }.BuildEntry());

            var entryNode = FindNodeFor(entry, root);

            var selection = CreateForEntryNode(entryNode);
            var parentSelection = CreateParentSelection(selection);

            var map = root.MapSelections(parentSelection);

            Assert.True(map.ContainsKey(entryNode));
            Assert.Same(selection, map[entryNode].Single());
        }

        [Fact]
        public void MapSelections_GroupedEntryLink_Success()
        {
            var entry = CreateEntry();

            var root = TreeRoot.Create(new SubBuilder
            {
                Groups = new List<IGroup>
                {
                    new SubBuilder
                    {
                        EntryLinks = new List<IEntryLink>
                        {
                            LinkTo(entry)
                        }
                    }.BuildGroup()
                }
            }.BuildEntry());

            var entryNode = FindNodeFor(entry, root);

            var selection = CreateForEntryNode(entryNode);
            var parentSelection = CreateParentSelection(selection);

            var map = root.MapSelections(parentSelection);

            Assert.True(map.ContainsKey(entryNode));
            Assert.Same(selection, map[entryNode].Single());
        }

        [Fact]
        public void MapSelections_LinkGroupedEntry_Success()
        {
            var entry = CreateEntry();

            var root = TreeRoot.Create(new SubBuilder
            {
                GroupLinks = new List<IGroupLink>
                {
                    LinkTo(new SubBuilder
                    {
                        Entries = new List<IEntry>
                        {
                            entry
                        }
                    }.BuildGroup())
                }
            }.BuildEntry());

            var entryNode = FindNodeFor(entry, root);

            var selection = CreateForEntryNode(entryNode);
            var parentSelection = CreateParentSelection(selection);

            var map = root.MapSelections(parentSelection);

            Assert.True(map.ContainsKey(entryNode));
            Assert.Same(selection, map[entryNode].Single());
        }

        [Fact]
        public void MapSelections_LinkGroupedEntryLink_Success()
        {
            var entry = CreateEntry();

            var root = TreeRoot.Create(new SubBuilder
            {
                GroupLinks = new List<IGroupLink>
                {
                    LinkTo(new SubBuilder
                    {
                        EntryLinks = new List<IEntryLink>
                        {
                            LinkTo(entry)
                        }
                    }.BuildGroup())
                }
            }.BuildEntry());

            var entryNode = root.GroupNodes.First().EntryNodes.First(node => node.Entry == entry);

            var selection = CreateForEntryNode(entryNode);
            var parentSelection = CreateParentSelection(selection);

            var map = root.MapSelections(parentSelection);

            Assert.True(map.ContainsKey(entryNode));
            Assert.Same(selection, map[entryNode].Single());
        }

        [Fact]
        public void MapSelections_LinkGroupedNotLinkGroupedEntry_Success_OverloadWithContainer()
        {
            MapSelections_LinkGroupedNotLinkGroupedEntry_Success_Helper(TestMapping_OverloadWithContainer);
        }

        [Fact]
        public void MapSelections_LinkGroupedNotLinkGroupedEntry_Success_OverloadWithParentSelection()
        {
            MapSelections_LinkGroupedNotLinkGroupedEntry_Success_Helper(TestMapping_OverloadWithParentSelection);
        }

        [Fact]
        public void MapSelections_OverloadWithContainer_ArgParentNull_Fail()
        {
            var root = TreeRoot.Create(new SubBuilder().BuildEntry());

            Assert.Throws<ArgumentNullException>(() => { root.MapSelections((ISelectionNodeContainer) null); });
        }

        [Fact]
        public void MapSelections_OverloadWithContainer_ArgThisNull_Fail()
        {
            var selectionNodeContainer = Substitute.For<ISelectionNodeContainer>();

            Assert.Throws<ArgumentNullException>(() => { ((INode) null).MapSelections(selectionNodeContainer); });
        }

        [Fact]
        public void MapSelections_OverloadWithContainer_Empty_Success()
        {
            var entry = Substitute.For<IEntry>();
            var root = TreeRoot.Create(entry);
            var selectionNodeContainer = Substitute.For<ISelectionNodeContainer>();

            var map = root.MapSelections(selectionNodeContainer);

            Assert.Equal(0, map.Count);
        }

        [Fact]
        public void MapSelections_OverloadWithSelection_ArgParentNull_Fail()
        {
            var root = TreeRoot.Create(new SubBuilder().BuildEntry());

            Assert.Throws<ArgumentNullException>(() => { root.MapSelections(null); });
        }

        [Fact]
        public void MapSelections_OverloadWithSelection_ArgThisNull_Fail()
        {
            var parentSelection = Substitute.For<ISelection>();

            Assert.Throws<ArgumentNullException>(() => { ((INode) null).MapSelections(parentSelection); });
        }

        [Fact]
        public void MapSelections_OverloadWithSelection_Empty_Success()
        {
            var entry = Substitute.For<IEntry>();
            var root = TreeRoot.Create(entry);
            var parentSelection = Substitute.For<ISelection>();

            var map = root.MapSelections(parentSelection);

            Assert.Equal(0, map.Count);
        }

        private static IEntry CreateEntry()
        {
            var id = Guid.NewGuid();
            var entry = Substitute.For<IEntry>();
            entry.Id.Value.Returns(id);
            return entry;
        }

        private static ISelection CreateForEntryNode(IEntryNode entryNode)
        {
            var linkParents =
                entryNode.Parents()
                    .PrependWith(entryNode)
                    .Where(node => node.IsLinkNode && (node.IsEntryNode || node.IsGroupNode))
                    .ToList();
            linkParents.Reverse();

            var entry = entryNode.Entry;
            var entryId = entry.Id.Value;
            var entryPath =
                linkParents.Select(
                    node => (node.IsEntryNode ? node.AsEntryNode.Link.Id : node.AsGroupNode.Link.Id).Value)
                    .Select(guid =>
                    {
                        var multiLink = Substitute.For<IMultiLink>();
                        multiLink.TargetId.Value.Returns(guid);
                        return multiLink;
                    })
                    .ToList();

            var parentGroup = entryNode.Parents().FirstOrDefault(node => node.IsGroupNode)?.AsGroupNode.Group;
            var parentGroupId = parentGroup?.Id.Value ?? Guid.Empty;
            var groupPath =
                linkParents.Where(node => node.IsGroupNode)
                    .Select(node => node.AsGroupNode.Link.Id.Value)
                    .Select(guid =>
                    {
                        var multiLink = Substitute.For<IMultiLink>();
                        multiLink.TargetId.Value.Returns(guid);
                        return multiLink;
                    })
                    .ToList();
            var selection = Substitute.For<ISelection>();

            selection.OriginEntryPath.Target.Returns(entry);
            selection.OriginEntryPath.TargetId.Value.Returns(entryId);
            selection.OriginEntryPath.Path.Returns(entryPath);

            selection.OriginGroupPath.Target.Returns(parentGroup);
            selection.OriginGroupPath.TargetId.Value.Returns(parentGroupId);
            selection.OriginGroupPath.Path.Returns(groupPath);

            return selection;
        }

        private static IGroup CreateGroup()
        {
            var id = Guid.NewGuid();
            var group = Substitute.For<IGroup>();
            group.Id.Value.Returns(id);
            return group;
        }

        private static ISelection CreateParentSelection(params ISelection[] selections)
        {
            var parentSelection = Substitute.For<ISelection>();
            parentSelection.Selections.Returns(new ReadonlyNode<ISelection, CataloguePath>(selections));
            return parentSelection;
        }

        private static ISelectionNodeContainer CreateSelectionNodeContainer(params ISelection[] selections)
        {
            var selectionNodeContainer = Substitute.For<ISelectionNodeContainer>();
            selectionNodeContainer.Selections.Returns(new ReadonlyNode<ISelection, CataloguePath>(selections));
            return selectionNodeContainer;
        }

        private static IEntryNode FindNodeFor(IEntry entry, INode root)
        {
            return
                root.AllDescendants(node => node.Children)
                    .Where(node => node.IsEntryNode)
                    .Select(node => node.AsEntryNode)
                    .First(node => node.Entry == entry);
        }

        private static IEntryLink LinkTo(IEntry entry)
        {
            var entryId = entry.Id.Value;
            var link = Substitute.For<IEntryLink>();
            link.Target.Returns(entry);
            link.TargetId.Value.Returns(entryId);
            link.Id.Value.Returns(Guid.NewGuid());
            return link;
        }

        private static IGroupLink LinkTo(IGroup group)
        {
            var groupId = group.Id.Value;
            var link = Substitute.For<IGroupLink>();
            link.Target.Returns(group);
            link.TargetId.Value.Returns(groupId);
            link.Id.Value.Returns(Guid.NewGuid());
            return link;
        }

        private static void MapSelections_LinkGroupedNotLinkGroupedEntry_Success_Helper(
            TestSingleSelectionMapping testSingleSelectionMapping)
        {
            var entry = CreateEntry();
            var root = TreeRoot.Create(new SubBuilder
            {
                GroupLinks = new List<IGroupLink>
                {
                    LinkTo(new SubBuilder
                    {
                        Groups = new List<IGroup>
                        {
                            new SubBuilder
                            {
                                Entries = new List<IEntry>
                                {
                                    entry
                                }
                            }.BuildGroup()
                        }
                    }.BuildGroup())
                }
            }.BuildEntry());

            testSingleSelectionMapping(entry, root);
        }

        private static void TestMapping_OverloadWithContainer(IEntry entry,
            INode root)
        {
            var entryNode = FindNodeFor(entry, root);

            var selection = CreateForEntryNode(entryNode);
            var selectionNodeContainer = CreateSelectionNodeContainer(selection);

            var map = root.MapSelections(selectionNodeContainer);

            Assert.True(map.ContainsKey(entryNode));
            Assert.Same(selection, map[entryNode].Single());
        }

        private static void TestMapping_OverloadWithParentSelection(IEntry entry,
            INode root)
        {
            var entryNode = FindNodeFor(entry, root);

            var selection = CreateForEntryNode(entryNode);
            var parentSelection = CreateParentSelection(selection);

            var map = root.MapSelections(parentSelection);

            Assert.True(map.ContainsKey(entryNode));
            Assert.Same(selection, map[entryNode].Single());
        }

        private delegate void TestSingleSelectionMapping(IEntry entry, INode root);

        public class SubBuilder
        {
            public List<IEntry> Entries { get; set; } = new List<IEntry>(0);

            public List<IEntryLink> EntryLinks { get; set; } = new List<IEntryLink>(0);

            public List<IGroupLink> GroupLinks { get; set; } = new List<IGroupLink>(0);

            public List<IGroup> Groups { get; set; } = new List<IGroup>(0);

            public IEntry BuildEntry()
            {
                var entry = CreateEntry();
                entry.Entries.Returns(new ReadonlyNodeSimple<IEntry>(Entries));
                entry.EntryLinks.Returns(new ReadonlyNode<IEntryLink, IEntry>(EntryLinks));
                entry.Groups.Returns(new ReadonlyNodeSimple<IGroup>(Groups));
                entry.GroupLinks.Returns(new ReadonlyNode<IGroupLink, IGroup>(GroupLinks));
                return entry;
            }

            public IGroup BuildGroup()
            {
                var group = CreateGroup();
                group.Entries.Returns(new ReadonlyNodeSimple<IEntry>(Entries));
                group.EntryLinks.Returns(new ReadonlyNode<IEntryLink, IEntry>(EntryLinks));
                group.Groups.Returns(new ReadonlyNodeSimple<IGroup>(Groups));
                group.GroupLinks.Returns(new ReadonlyNode<IGroupLink, IGroup>(GroupLinks));
                return group;
            }
        }
    }
}
