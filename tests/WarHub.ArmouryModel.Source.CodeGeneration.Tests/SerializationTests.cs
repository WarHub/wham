using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using Xunit;

namespace WarHub.ArmouryModel.Source.CodeGeneration.Tests
{
    public class SerializationTests
    {
        [Fact]
        public void CheckEmptyDeserialization()
        {
            const string ContainerXml = "<container/>";
            var serializer = new XmlSerializer(typeof(ContainerCore.Builder));
            using var reader = XmlReader.Create(new StringReader(ContainerXml));
            var container = (ContainerCore.Builder)serializer.Deserialize(reader)!;
            Assert.Null(container.Id);
            Assert.Null(container.Name);
            Assert.Empty(container.Items);
            Assert.True(container.ToImmutable().Items.IsEmpty);
        }

        [Fact]
        public void ProfileTypeSerializesAndDeserializes()
        {
            var emptyGuid = Guid.Empty.ToString();
            const string ContainerName = "Container0";
            const string Item1Name = "Item1";
            const string Item2Name = "Item2";
            const string Item3Name = "Item3";
            var profileType = new ContainerCore.Builder
            {
                Id = emptyGuid,
                Name = ContainerName,
                Items =
                {
                    new ItemCore.Builder
                    {
                        Id = emptyGuid,
                        Name = Item1Name
                    },
                    new ItemCore.Builder
                    {
                        Id = emptyGuid,
                        Name = Item2Name
                    },
                    new ItemCore.Builder
                    {
                        Id = emptyGuid,
                        Name = Item3Name
                    }
                },
            }.ToImmutable();
            var serializer = new XmlSerializer(typeof(ContainerCore.FastSerializationProxy));
            using var stream = new MemoryStream();
            serializer.Serialize(stream, profileType.ToSerializationProxy());

            stream.Position = 0;

            using var reader = XmlReader.Create(stream);
            var deserialized = (ContainerCore.Builder)new XmlSerializer(typeof(ContainerCore.Builder)).Deserialize(reader)!;

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
