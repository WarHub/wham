using System;
using System.IO;
using System.Xml.Serialization;
using Xunit;

namespace WarHub.ArmouryModel.Source.Tests
{
    public class SerializationTestBase : IClassFixture<SerializationTestBase.XmlDataFixture>
    {
        protected RosterNode DeserializeRoster(Stream stream)
        {
            var serializer = new XmlSerializer(typeof(RosterCore.Builder));
            var builder = (RosterCore.Builder)serializer.Deserialize(stream);
            return builder.ToImmutable().ToNode();
        }

        protected CatalogueNode DeserializeCatalogue(Stream stream)
        {
            var serializer = new XmlSerializer(typeof(CatalogueCore.Builder));
            var builder = (CatalogueCore.Builder)serializer.Deserialize(stream);
            return builder.ToImmutable().ToNode();
        }

        protected GameSystemNode DeserializeGameSystem(Stream stream)
        {
            var serializer = new XmlSerializer(typeof(GameSystemCore.Builder));
            var builder = (GameSystemCore.Builder)serializer.Deserialize(stream);
            return builder.ToImmutable().ToNode();
        }

        protected void SerializeGameSystem(SourceNode node, Stream stream)
        {
            var serializer = new XmlSerializer(typeof(GameSystemCore.FastSerializationProxy));
            var castNode = (GameSystemNode)node;
            var fse = castNode.Core.ToSerializationProxy();
            serializer.Serialize(stream, fse);
        }

        protected void SerializeCatalogue(SourceNode node, Stream stream)
        {
            var serializer = new XmlSerializer(typeof(CatalogueCore.FastSerializationProxy));
            var castNode = (CatalogueNode)node;
            var fse = castNode.Core.ToSerializationProxy();
            serializer.Serialize(stream, fse);
        }

        protected void SerializeRoster(SourceNode node, Stream stream)
        {
            var serializer = new XmlSerializer(typeof(RosterCore.FastSerializationProxy));
            var castNode = (RosterNode)node;
            var fse = castNode.Core.ToSerializationProxy();
            serializer.Serialize(stream, fse);
        }

        public static class XmlTestData
        {
            public const string InputDir = "XmlTestDatafiles";
            public const string OutputDir = "XmlTestsOutput";
            public const string GameSystemFilename = "Warhammer 40,000 8th Edition.gst";
            public const string Catalogue1Filename = "T'au Empire.cat";
            public const string Catalogue2Filename = "Imperium - Space Marines.cat";
            public const string RosterFilename = "Wham Demo Test Roster.ros";
        }

        public class XmlDataFixture : IDisposable
        {
            public XmlDataFixture()
            {
                CreateDir();
                // ReSharper disable once UnusedVariable
                var serializers = new[]
                {
                    new XmlSerializer(typeof(GameSystemCore.Builder)),
                    new XmlSerializer(typeof(CatalogueCore.Builder)),
                    new XmlSerializer(typeof(RosterCore.Builder)),
                    new XmlSerializer(typeof(GameSystemCore.FastSerializationProxy)),
                    new XmlSerializer(typeof(CatalogueCore.FastSerializationProxy)),
                    new XmlSerializer(typeof(RosterCore.FastSerializationProxy))
                };
            }

            public void Dispose()
            {
                RemoveDir();
            }

            private static void CreateDir()
            {
                Directory.CreateDirectory(XmlTestData.OutputDir);
            }

            private static void RemoveDir()
            {
                Directory.Delete(XmlTestData.OutputDir, true);
            }
        }
    }
}
