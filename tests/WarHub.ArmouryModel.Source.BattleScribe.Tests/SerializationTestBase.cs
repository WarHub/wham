using System;
using System.IO;
using System.Xml.Serialization;
using Xunit;

namespace WarHub.ArmouryModel.Source.BattleScribe.Tests
{
    public class SerializationTestBase : IClassFixture<SerializationTestBase.XmlDataFixture>
    {
        internal static class XmlTestData
        {
            public const string InputDir = "XmlTestDatafiles";
            public const string OutputDir = "XmlTestsOutput";
            public const string GamesystemFilename = "Warhammer 40,000 8th Edition.gst";
            public const string Catalogue1Filename = "T'au Empire.cat";
            public const string Catalogue2Filename = "Imperium - Space Marines.cat";
            public const string RosterFilename = "Wham Demo Test Roster.ros";
        }

#pragma warning disable CA1812 // Avoid uninstantiated internal classes
        internal sealed class XmlDataFixture : IDisposable
#pragma warning restore CA1812 // Avoid uninstantiated internal classes
        {
            public XmlDataFixture()
            {
                CreateDir();
                _ = new[]
                {
                    new XmlSerializer(typeof(GamesystemCore.Builder)),
                    new XmlSerializer(typeof(CatalogueCore.Builder)),
                    new XmlSerializer(typeof(RosterCore.Builder)),
                    new XmlSerializer(typeof(GamesystemCore.FastSerializationProxy)),
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
