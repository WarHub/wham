using System.Collections.Immutable;
using FluentAssertions;
using WarHub.ArmouryModel.ProjectModel;
using WarHub.ArmouryModel.Source;
using Xunit;

namespace WarHub.ArmouryModel.Workspaces.BattleScribe.Tests
{
    public class XmlWorkspaceTests
    {
        [Fact]
        public void CanCreateEmptyWorkspaceFromDocuments()
        {
            var ws = XmlWorkspace.CreateFromDocuments(ImmutableArray<XmlDocument>.Empty);

            ws.Should().NotBeNull();
        }

        [Fact]
        public void CanCreateWorkspaceWithInMemoryFile()
        {
            var node = NodeFactory.Gamesystem();
            var file = XmlDocument.Create(DatafileInfo.Create("test.gst", node));

            var ws = XmlWorkspace.CreateFromDocuments(ImmutableArray.Create(file));

            ws.DocumentsByKind[XmlDocumentKind.Gamesystem].Should().ContainSingle();
        }
    }
}
