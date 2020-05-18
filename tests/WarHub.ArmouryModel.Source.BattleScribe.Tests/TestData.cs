using System.IO;
using System.Linq;
using System.Xml.Serialization;
using WarHub.ArmouryModel.Source.XmlFormat;

namespace WarHub.ArmouryModel.Source.BattleScribe.Tests
{
    internal static class TestData
    {
        public const string InputDir = "XmlTestDatafiles/Grim and Dark Future";
        public const string Gamesystem = "Grim and Dark Future.gst";
        public const string Catalogue = "Warriors in Warsuits.cat";
        public const string Roster = "Small Battalion of Warriors.ros";

        static TestData()
        {
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

        public static string GetDatafilePath(this string datafileName, BattleScribeVersion? version = null)
        {
            // null version is latest:
            var versionResolved = version ?? BattleScribeVersion.WellKnownVersions.Last();
            var versionText = versionResolved.FilepathString;
            return Path.Combine(InputDir, "v" + versionText, datafileName + ".xml");
        }

        public static Stream GetDatafileStream(this string datafileName, BattleScribeVersion? version = null)
        {
            var path = GetDatafilePath(datafileName, version);
            return File.OpenRead(path);
        }
    }
}
