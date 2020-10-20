using System.IO;
using System.Linq;
using WarHub.ArmouryModel.Source.XmlFormat;

namespace WarHub.ArmouryModel.Source.BattleScribe.Tests
{
    internal static class TestData
    {
        public const string InputDir = "XmlTestDatafiles/Grim and Dark Future";
        public const string Gamesystem = "Grim and Dark Future.gst";
        public const string Catalogue = "Warriors in Warsuits.cat";
        public const string Roster = "Small Battalion of Warriors.ros";

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
