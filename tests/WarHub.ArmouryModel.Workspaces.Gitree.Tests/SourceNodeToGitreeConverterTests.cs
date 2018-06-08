using System;
using System.Collections.Generic;
using System.Text;
using WarHub.ArmouryModel.Source;
using Xunit;

namespace WarHub.ArmouryModel.Workspaces.Gitree.Tests
{
    public class SourceNodeToGitreeConverterTests
    {
        [Fact]
        public void AddingToEmptyBlob_AddsToCorrectList()
        {
            var addedCore = new CharacteristicTypeCore.Builder
            {
                Id = "id",
                Name = "name"
            }.ToImmutable();
            var result = SourceNodeToGitreeConverter.AddingToEmptyBlobRewriter.AddToEmpty(addedCore.ToNode());
            Assert.Collection(result.CharacteristicTypes,
                x => Assert.Same(addedCore, ((INodeWithCore<CharacteristicTypeCore>)x).Core));
        }

        [Fact]
        public void FolderKindDroppingRewriter_ClearsCorrectList()
        {
            var characteristicType = NodeFactory.CharacteristicType("id", "name");
            var profile = NodeFactory.ProfileType("id", "name", NodeList.Create(characteristicType));
            var rewriter = new SourceNodeToGitreeConverter.FolderKindDroppingRewriter();
            var result = (DatablobNode)NodeFactory.Datablob(
                NodeFactory.Metadata(null, null, null),
                characteristicTypes: NodeFactory.CharacteristicTypeList(characteristicType),
                profileTypes: NodeFactory.ProfileTypeList(profile))
                .Accept(rewriter);
            Assert.Collection(result.CharacteristicTypes, x => Assert.NotNull(x));
            Assert.Empty(result.ProfileTypes);
        }
    }
}
