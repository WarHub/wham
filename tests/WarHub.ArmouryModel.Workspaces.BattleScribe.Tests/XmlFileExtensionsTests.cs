using System;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using WarHub.ArmouryModel.ProjectModel;
using WarHub.ArmouryModel.Source;
using Xunit;

namespace WarHub.ArmouryModel.Workspaces.BattleScribe.Tests
{
    public class XmlFileExtensionsTests
    {
        private const string DataIndexFilename = "index.xml";

        [Fact]
        public void ReadExistingRepoDistribution()
        {
            const string RepoFilename = "Files/repo.bsr";
            using var fileStream = File.OpenRead(RepoFilename);
            var repo = fileStream.ReadRepoDistribution();
            Assert.Equal(DataIndexFilename, repo.Index.Filepath);
            Assert.Equal(
                "A_Song_Of_Ice_and_Fire_Miniatures_Game.gst",
                repo.Datafiles.OfType<IDatafileInfo<GamesystemNode>>().Single().Filepath);
            Assert.Equal(3, repo.Datafiles.OfType<IDatafileInfo<CatalogueNode>>().Count());
        }

        [Fact]
        public async Task WriteRepoDistributionAsync()
        {
            const string DataIndexName = "Test dataindex";
            var gstId = Guid.NewGuid().ToString();
            var catId1 = Guid.NewGuid().ToString();
            var catId2 = Guid.NewGuid().ToString();
            var gstNode = NodeFactory.Gamesystem(id: gstId);
            var original =
                new RepoDistribution(
                    DatafileInfo.Create(
                        DataIndexFilename,
                        NodeFactory.DataIndex(DataIndexName)),
                    (new IDatafileInfo<CatalogueBaseNode>[]
                    {
                        DatafileInfo.Create("gamesystem.gst", gstNode),
                        DatafileInfo.Create("cat1.cat", NodeFactory.Catalogue(gstNode, id: catId1)),
                        DatafileInfo.Create("cat2.cat", NodeFactory.Catalogue(gstNode, id: catId2)),
                    })
                    .ToImmutableArray());
            using var memory = new MemoryStream();
            await original.WriteToAsync(memory);
            memory.Position = 0;
            var read = memory.ReadRepoDistribution();
            Assert.Equal(DataIndexName, read.Index.Node!.Name);
            Assert.True(
                read.Datafiles
                .Select(x => x.Node!.Id)
                .ToHashSet()
                .SetEquals(new[] { gstId, catId1, catId2 }));
        }

        [Theory]
        [InlineData("data.gstz")]
        [InlineData("data.catz")]
        [InlineData("data.rosz")]
        [InlineData("index.bsi")]
        [InlineData("repo.bsr")]
        [InlineData("some/dir/data.catz")]
        [InlineData("DATA.CATZ")]
        public void ZippedPathsAreDetectedAsZipped(string path)
        {
            Assert.True(path.IsXmlZippedPath());
        }

        [Theory]
        [InlineData("data.gst")]
        [InlineData("data.cat")]
        [InlineData("data.ros")]
        [InlineData("index.xml")]
        [InlineData("some/dir/data.cat")]
        [InlineData("notadatafile.txt")]
        [InlineData("noextension")]
        public void PlainPathsAreNotDetectedAsZipped(string path)
        {
            Assert.False(path.IsXmlZippedPath());
        }

        [Theory]
        [InlineData(XmlDocumentKind.Unknown)]
        [InlineData(XmlDocumentKind.Gamesystem)]
        [InlineData(XmlDocumentKind.Catalogue)]
        [InlineData(XmlDocumentKind.Roster)]
        [InlineData(XmlDocumentKind.DataIndex)]
        [InlineData(XmlDocumentKind.RepoDistribution)]
        public void IsXmlZippedOnKindDoesNotThrow(XmlDocumentKind kind)
        {
            // Unknown used to throw KeyNotFoundException here.
            var exception = Record.Exception(() => kind.IsXmlZipped());
            Assert.Null(exception);
        }

        [Fact]
        public void OnlyRepoDistributionIsAlwaysZipped()
        {
            // Every other kind has both a plain and a zipped extension, so asking
            // whether the kind is zipped can only be answered for .bsr.
            Assert.True(XmlDocumentKind.RepoDistribution.IsXmlZipped());
            Assert.False(XmlDocumentKind.Catalogue.IsXmlZipped());
            Assert.False(XmlDocumentKind.Gamesystem.IsXmlZipped());
            Assert.False(XmlDocumentKind.Roster.IsXmlZipped());
            Assert.False(XmlDocumentKind.DataIndex.IsXmlZipped());
            Assert.False(XmlDocumentKind.Unknown.IsXmlZipped());
        }

        [Fact]
        public async Task ZippedCatalogueRoundTripsThroughLoadSourceAuto()
        {
            // The regression in #311: the library could write a .catz but not read it
            // back. LoadSourceAuto handed the raw ZIP bytes to the XML reader, which
            // failed with "Data at the root level is invalid".
            var gstNode = NodeFactory.Gamesystem(id: Guid.NewGuid().ToString());
            var catId = Guid.NewGuid().ToString();
            var datafile = DatafileInfo.Create(
                "roundtrip.cat",
                NodeFactory.Catalogue(gstNode, name: "Round Trip", id: catId));

            var directory = Directory.CreateTempSubdirectory("wham-catz-roundtrip");
            try
            {
                var filepath = Path.Combine(directory.FullName, "roundtrip.catz");
                await datafile.WriteXmlZippedFileAsync(filepath);

                using var readStream = File.OpenRead(filepath);
                var node = readStream.LoadSourceAuto(filepath);

                var catalogue = Assert.IsAssignableFrom<CatalogueNode>(node);
                Assert.Equal(catId, catalogue.Id);
                Assert.Equal("Round Trip", catalogue.Name);
            }
            finally
            {
                directory.Delete(recursive: true);
            }
        }

        [Fact]
        public async Task PlainCatalogueStillRoundTripsThroughLoadSourceAuto()
        {
            var gstNode = NodeFactory.Gamesystem(id: Guid.NewGuid().ToString());
            var catId = Guid.NewGuid().ToString();
            var datafile = DatafileInfo.Create(
                "roundtrip.cat",
                NodeFactory.Catalogue(gstNode, name: "Round Trip", id: catId));

            var directory = Directory.CreateTempSubdirectory("wham-cat-roundtrip");
            try
            {
                var filepath = Path.Combine(directory.FullName, "roundtrip.cat");
                await datafile.WriteXmlFileAsync(filepath);

                using var readStream = File.OpenRead(filepath);
                var node = readStream.LoadSourceAuto(filepath);

                var catalogue = Assert.IsAssignableFrom<CatalogueNode>(node);
                Assert.Equal(catId, catalogue.Id);
            }
            finally
            {
                directory.Delete(recursive: true);
            }
        }
    }
}
