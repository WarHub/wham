using System.Collections.Immutable;
using System.IO;
using System.Linq;

namespace WarHub.ArmouryModel.Source.XmlFormat
{
    public static class Resources
    {
        private const string XsdResourceFormat = ThisAssembly.RootNamespace + ".DataFormat.xml.schema.latest.{0}.xsd";
        private const string XslTransformResourceFormat = ThisAssembly.RootNamespace + ".DataFormat.xml.transform.{0}_{1}.xsl";

        public static ImmutableDictionary<RootElement, ImmutableSortedSet<VersionedElementInfo>> XslMigrations { get; }
            = (from version in new[] { BattleScribeVersion.V1_15, BattleScribeVersion.V2_00, BattleScribeVersion.V2_01, BattleScribeVersion.V2_02 }
               from element in new[] { RootElement.GameSystem, RootElement.Catalogue }
               select new VersionedElementInfo(element, version))
            .Append(new VersionedElementInfo(RootElement.DataIndex, BattleScribeVersion.V2_02))
            .GroupBy(x => x.Element)
            .ToImmutableDictionary(x => x.Key, x => x.ToImmutableSortedSet());

        private static string GetMigrationResourcePath(this VersionedElementInfo elementInfo) =>
            string.Format(
                XslTransformResourceFormat,
                elementInfo.Element,
                elementInfo.Version.FilepathString);

        private static string GetXsdResourcePath(this RootElement rootElement) =>
            string.Format(XsdResourceFormat, rootElement);

        public static Stream OpenXsdStream(this RootElement rootElement)
        {
            return OpenResource(rootElement.GetXsdResourcePath());
        }

        private static Stream OpenXsdStream(this VersionedElementInfo elementInfo)
        {
            return OpenResource(elementInfo.Element.GetXsdResourcePath());
        }

        public static Stream OpenMigrationXslStream(this VersionedElementInfo elementInfo)
        {
            return OpenResource(elementInfo.GetMigrationResourcePath());
        }

        private static Stream OpenResource(string name)
            => typeof(Resources).Assembly.GetManifestResourceStream(name);
    }
}
