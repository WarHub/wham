using System;
using System.Collections.Immutable;
using System.IO;
using System.Xml;
using Xunit;

namespace WarHub.ArmouryModel.Source.CodeGeneration.Tests
{
    public class SerializationTests
    {
        [Fact]
        public void CheckEmptyDeserialization()
        {
            const string ContainerXml = "<container/>";
            var serializer = new XmlFormat.ContainerCoreXmlSerializer();
            using var reader = XmlReader.Create(new StringReader(ContainerXml));
            var container = (ContainerCore)serializer.Deserialize(reader)!;
            Assert.Null(container.Id);
            Assert.Null(container.Name);
            Assert.Empty(container.Items);
            Assert.True(container.Items.IsEmpty);
        }

        [Fact]
        public void ContainerSerializesAndDeserializes()
        {
            var emptyGuid = Guid.Empty.ToString();
            const string ContainerName = "Container0";
            const string Item1Name = "Item1";
            const string Item2Name = "Item2";
            const string Item3Name = "Item3";
            var profileType = new ContainerCore
            {
                Id = emptyGuid,
                Name = ContainerName,
                Items = ImmutableArray.Create(
                    new ItemCore
                    {
                        Id = emptyGuid,
                        Name = Item1Name
                    },
                    new ItemCore
                    {
                        Id = emptyGuid,
                        Name = Item2Name
                    },
                    new ItemCore
                    {
                        Id = emptyGuid,
                        Name = Item3Name
                    })
            };
            var serializer = new XmlFormat.ContainerCoreXmlSerializer();
            using var stream = new MemoryStream();
            serializer.Serialize(stream, profileType);

            stream.Position = 0;

            using var reader = XmlReader.Create(stream);
            var deserialized = (ContainerCore)serializer.Deserialize(reader)!;

            Assert.NotNull(deserialized);
            Assert.Equal(ContainerName, deserialized.Name);
            Assert.Collection(
                deserialized.Items,
                x => Assert.Equal(Item1Name, x.Name),
                x => Assert.Equal(Item2Name, x.Name),
                x => Assert.Equal(Item3Name, x.Name));
        }
    }
}
