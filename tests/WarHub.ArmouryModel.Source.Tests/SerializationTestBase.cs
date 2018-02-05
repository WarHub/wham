using System;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using Xunit;

namespace WarHub.ArmouryModel.Source.Tests
{
    public class SerializationTestBase : IClassFixture<SerializationTestBase.XmlDataFixture>
    {
        protected XmlWriterSettings XmlWriterSettings => LazyXmlWriterSettings.Value;

        protected XmlSerializerNamespaces RosterNamespaces => LazyRosterNamespaces.Value;

        protected XmlSerializerNamespaces CatalogueNamespaces => LazyCatalogueNamespaces.Value;

        protected XmlSerializerNamespaces GameSystemNamespaces => LazyGameSystemNamespaces.Value;

        private Lazy<XmlWriterSettings> LazyXmlWriterSettings { get; }
            = new Lazy<XmlWriterSettings>(CreateXmlWriterSettings);

        private static XmlWriterSettings CreateXmlWriterSettings()
        {
            return BattleScribeXml.XmlWriterSettings;
        }

        private Lazy<XmlSerializerNamespaces> LazyRosterNamespaces { get; }
            = new Lazy<XmlSerializerNamespaces>(CreateRosterXmlSerializerNamespaces);

        private Lazy<XmlSerializerNamespaces> LazyCatalogueNamespaces { get; }
            = new Lazy<XmlSerializerNamespaces>(CreateCatalogueXmlSerializerNamespaces);

        private Lazy<XmlSerializerNamespaces> LazyGameSystemNamespaces { get; }
            = new Lazy<XmlSerializerNamespaces>(CreateGameSystemXmlSerializerNamespaces);

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
            using (var writer = XmlWriter.Create(stream, XmlWriterSettings))
            {
                serializer.Serialize(writer, fse, GameSystemNamespaces);
            }
        }

        protected void SerializeCatalogue(SourceNode node, Stream stream)
        {
            var serializer = new XmlSerializer(typeof(CatalogueCore.FastSerializationProxy));
            var castNode = (CatalogueNode)node;
            var fse = castNode.Core.ToSerializationProxy();
            using (var writer = XmlWriter.Create(stream, XmlWriterSettings))
            {
                serializer.Serialize(writer, fse, CatalogueNamespaces);
            }
        }

        protected void SerializeRoster(SourceNode node, Stream stream)
        {
            var serializer = new XmlSerializer(typeof(RosterCore.FastSerializationProxy));
            var castNode = (RosterNode)node;
            var fse = castNode.Core.ToSerializationProxy();
            using (var writer = XmlWriter.Create(stream, XmlWriterSettings))
            {
                serializer.Serialize(writer, fse, RosterNamespaces);
            }
        }

        private static XmlSerializerNamespaces CreateRosterXmlSerializerNamespaces()
        {
            var namespaces = new XmlSerializerNamespaces();
            namespaces.Add("", RosterCore.RosterXmlNamespace);
            return namespaces;
        }

        private static XmlSerializerNamespaces CreateCatalogueXmlSerializerNamespaces()
        {
            var namespaces = new XmlSerializerNamespaces();
            namespaces.Add("", CatalogueCore.CatalogueXmlNamespace);
            return namespaces;
        }

        private static XmlSerializerNamespaces CreateGameSystemXmlSerializerNamespaces()
        {
            var namespaces = new XmlSerializerNamespaces();
            namespaces.Add("", GameSystemCore.GameSystemXmlNamespace);
            return namespaces;
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
                //Directory.Delete(XmlTestData.OutputDir, true);
            }
        }
    }
}
