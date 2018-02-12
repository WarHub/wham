using System;
using System.IO;
using System.Xml.Serialization;
using WarHub.ArmouryModel.Source.CodeGeneration.Tests.GeneratedCode;
using Xunit;

namespace WarHub.ArmouryModel.Source.CodeGeneration.Tests
{
    public class SerializationTests
    {
        [Fact]
        public void CheckEmptyDeserialization()
        {
            var containerXml = "<container/>";
            var serializer = new XmlSerializer(typeof(ContainerCore.Builder));
            var container = (ContainerCore.Builder)serializer.Deserialize(new StringReader(containerXml));
            Assert.Null(container.Id);
            Assert.Null(container.Name);
            Assert.Empty(container.Items);
            Assert.True(container.ToImmutable().Items.IsEmpty);
        }

        [Fact]
        public void ProfileTypeSerializesAndDeserializes()
        {
            string emptyGuid = Guid.Empty.ToString();
            const string containerName = "Container0";
            const string item1Name = "Item1";
            const string item2Name = "Item2";
            const string item3Name = "Item3";
            var profileType = new ContainerCore.Builder
            {
                Id = emptyGuid,
                Name = containerName,
                Items =
                {
                    new ItemCore.Builder
                    {
                        Id = emptyGuid,
                        Name = item1Name
                    },
                    new ItemCore.Builder
                    {
                        Id = emptyGuid,
                        Name = item2Name
                    },
                    new ItemCore.Builder
                    {
                        Id = emptyGuid,
                        Name = item3Name
                    }
                },
            }.ToImmutable();
            XmlSerializer serializer = new XmlSerializer(typeof(ContainerCore.FastSerializationProxy));
            using (var stream = new MemoryStream())
            {
                serializer.Serialize(stream, profileType.ToSerializationProxy());

                stream.Position = 0;

                var deserialized = (ContainerCore.Builder) new XmlSerializer(typeof(ContainerCore.Builder)).Deserialize(stream);


                Assert.NotNull(deserialized);
                Assert.Equal(containerName, deserialized.Name);
                Assert.Collection(
                    deserialized.Items,
                    x => Assert.Equal(item1Name, x.Name),
                    x => Assert.Equal(item2Name, x.Name),
                    x => Assert.Equal(item3Name, x.Name));
            }
        }
    }
}
