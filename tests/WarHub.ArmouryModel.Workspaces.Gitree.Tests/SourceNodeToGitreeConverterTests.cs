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
    }
}
