﻿using System.Collections.Immutable;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace WarHub.ArmouryModel.Source.XmlFormat
{
    public static class XmlResources
    {
        private static CompositeFormat XsdResourceFormat { get; } =
            CompositeFormat.Parse(ThisAssembly.RootNamespace + ".DataFormat.xml.schema.latest.{0}.xsd");
        private static CompositeFormat XslTransformResourceFormat { get; } =
            CompositeFormat.Parse(ThisAssembly.RootNamespace + ".DataFormat.xml.transform.{0}_{1}.xsl");

        private static ImmutableArray<BattleScribeVersion> CatGstRosMigrations { get; } =
            ImmutableArray.Create(
                BattleScribeVersion.V1x15,
                BattleScribeVersion.V2x00,
                BattleScribeVersion.V2x01,
                BattleScribeVersion.V2x02,
                BattleScribeVersion.V2x03);

        public static ImmutableDictionary<RootElement, ImmutableSortedSet<VersionedElementInfo>> XslMigrations { get; }
            = (from version in CatGstRosMigrations
               from element in new[] { RootElement.GameSystem, RootElement.Catalogue, RootElement.Roster }
               select new VersionedElementInfo(element, version))
            .Append(new VersionedElementInfo(RootElement.DataIndex, BattleScribeVersion.V2x02))
            .GroupBy(x => x.Element)
            .ToImmutableDictionary(x => x.Key, x => x.ToImmutableSortedSet());

        private static string GetMigrationResourcePath(this VersionedElementInfo elementInfo) =>
            string.Format(
                CultureInfo.InvariantCulture,
                XslTransformResourceFormat,
                elementInfo.Element,
                elementInfo.Version?.FilepathString);

        private static string GetXsdResourcePath(this RootElement rootElement) =>
            string.Format(CultureInfo.InvariantCulture, XsdResourceFormat, rootElement);

        public static Stream? OpenXsdStream(this RootElement rootElement)
        {
            return OpenResource(rootElement.GetXsdResourcePath());
        }

        public static Stream? OpenMigrationXslStream(this VersionedElementInfo elementInfo)
        {
            return OpenResource(elementInfo.GetMigrationResourcePath());
        }

        private static Stream? OpenResource(string name)
            => typeof(XmlResources).Assembly.GetManifestResourceStream(name);
    }
}
