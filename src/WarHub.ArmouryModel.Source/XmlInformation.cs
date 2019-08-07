using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;

namespace WarHub.ArmouryModel.Source
{
    public static class XmlInformation
    {
        private const string XsdVersion2_2ResourcePrefix = ThisAssembly.RootNamespace + ".DataFormat.xml.schema.v2_2.";
        private const string XslTransformResourceFormat = ThisAssembly.RootNamespace + ".DataFormat.xml.transform.{0}_{1}.xsl";

        public static class Namespaces
        {
            public const string CatalogueXmlns = "http://www.battlescribe.net/schema/catalogueSchema";
            public const string RosterXmlns = "http://www.battlescribe.net/schema/rosterSchema";
            public const string GamesystemXmlns = "http://www.battlescribe.net/schema/gameSystemSchema";
            public const string DataIndexXmlns = "http://www.battlescribe.net/schema/dataIndexSchema";
        }

        public static class RootElementNames
        {
            public const string Catalogue = "catalogue";
            public const string DataIndex = "dataIndex";
            public const string GameSystem = "gameSystem";
            public const string Roster = "roster";
        }

        public enum RootElement
        {
            Catalogue,
            GameSystem,
            Roster,
            DataIndex
        }

        public enum BsDataVersion
        {
            Unknown,
            v1_15,
            v2_00,
            v2_01,
            v2_02
        }

        public static Stream OpenXsdStream(RootElement rootElement)
        {
            return OpenResource(GetResourceName());
            string GetResourceName()
            {
                switch (rootElement)
                {
                    case RootElement.Catalogue:
                        return XsdVersion2_2ResourcePrefix + "Catalogue.xsd";
                    case RootElement.GameSystem:
                        return XsdVersion2_2ResourcePrefix + "GameSystem.xsd";
                    case RootElement.Roster:
                        return XsdVersion2_2ResourcePrefix + "Roster.xsd";
                    default:
                        throw new ArgumentOutOfRangeException(nameof(rootElement));
                }
            }
        }

        public static Stream OpenMigrationXslStream(RootElement rootElement, BsDataVersion dataVersion)
        {
            return OpenResource(GetResourceName());
            string GetResourceName()
            {
                return string.Format(XslTransformResourceFormat, GetElementName(), dataVersion.Info().FilepathString);
            }
            string GetElementName()
            {
                switch (rootElement)
                {
                    case RootElement.Catalogue:
                        return "catalogue";
                    case RootElement.GameSystem:
                        return "game_system";
                    case RootElement.DataIndex:
                        return "data_index";
                    case RootElement.Roster:
                        return "roster";
                    default:
                        throw new ArgumentOutOfRangeException(nameof(rootElement));
                }
            }
        }

        private static Stream OpenResource(string name)
            => typeof(XmlInformation).Assembly.GetManifestResourceStream(name);

        public static ImmutableArray<BsDataVersion> BsDataVersions { get; }
            = new[]
            {
                BsDataVersion.v1_15,
                BsDataVersion.v2_00,
                BsDataVersion.v2_01,
                BsDataVersion.v2_02
            }.ToImmutableArray();

        public static BsDataVersionInfo Info(this BsDataVersion version)
            => new BsDataVersionInfo(version);

        public static RootElementInfo Info(this RootElement rootElement)
            => new RootElementInfo(rootElement);

        public static BsDataVersion ParseBsDataVersion(this string dataVersion)
            => BsDataVersionInfo.BsDataVersionFromString.TryGetValue(dataVersion, out var result)
                ? result
                : BsDataVersion.Unknown;

        public static RootElement ParseRootElement(this string xmlElementName)
            => RootElementInfo.RootElementFromXmlName[xmlElementName];

        public readonly struct BsDataVersionInfo
        {
            internal BsDataVersionInfo(BsDataVersion version)
            {
                Version = version;
            }

            public BsDataVersion Version { get; }

            public string DisplayString => DisplayStrings[Version];

            public string FilepathString => FilepathStrings[Version];

            internal static ImmutableDictionary<BsDataVersion, string> DisplayStrings { get; }
                = new Dictionary<BsDataVersion, string>
                {
                    [BsDataVersion.v1_15] = "1.15",
                    [BsDataVersion.v2_00] = "2.00",
                    [BsDataVersion.v2_01] = "2.01",
                    [BsDataVersion.v2_02] = "2.02"
                }.ToImmutableDictionary();

            internal static ImmutableDictionary<BsDataVersion, string> FilepathStrings { get; }
                = DisplayStrings
                .ToImmutableDictionary(x => x.Key, x => x.Value.Replace('.', '_'));

            internal static ImmutableDictionary<string, BsDataVersion> BsDataVersionFromString { get; }
                = DisplayStrings
                .ToImmutableDictionary(x => x.Value, x => x.Key);
        }

        public readonly struct RootElementInfo
        {
            internal RootElementInfo(RootElement element)
            {
                Element = element;
            }

            public RootElement Element { get; }

            public string Namespace => NamespaceFromElement[Element];

            public string XmlElementName => XmlNames[Element];

            public BsDataVersion CurrentVersion => BsDataVersion.v2_02;

            public IEnumerable<BsDataVersion> AvailableMigrations(BsDataVersion sourceVersion)
            {
                var self = this;
                return
                    sourceVersion != BsDataVersion.Unknown
                    && sourceVersion != self.CurrentVersion
                    && Migrations.TryGetValue(self.Element, out var migrationVersions)
                    ? migrationVersions.SkipWhile(x => x != sourceVersion)
                    : ImmutableList<BsDataVersion>.Empty;
            }

            internal static ImmutableDictionary<RootElement, string> NamespaceFromElement { get; }
                = new Dictionary<RootElement, string>
                {
                    [RootElement.Catalogue] = Namespaces.CatalogueXmlns,
                    [RootElement.DataIndex] = Namespaces.DataIndexXmlns,
                    [RootElement.GameSystem] = Namespaces.GamesystemXmlns,
                    [RootElement.Roster] = Namespaces.RosterXmlns,
                }.ToImmutableDictionary();

            internal static ImmutableDictionary<RootElement, ImmutableArray<BsDataVersion>> Migrations { get; }
                = new Dictionary<RootElement, ImmutableArray<BsDataVersion>>
                {
                    [RootElement.Catalogue] = BsDataVersions,
                    [RootElement.GameSystem] = BsDataVersions,
                }.ToImmutableDictionary();

            internal static ImmutableDictionary<string, RootElement> RootElementFromXmlName { get; }
                = new Dictionary<string, RootElement>
                {
                    [RootElementNames.Catalogue] = RootElement.Catalogue,
                    [RootElementNames.DataIndex] = RootElement.DataIndex,
                    [RootElementNames.GameSystem] = RootElement.GameSystem,
                    [RootElementNames.Roster] = RootElement.Roster,
                }.ToImmutableDictionary();

            internal static ImmutableDictionary<RootElement, string> XmlNames { get; }
                = RootElementFromXmlName
                .ToImmutableDictionary(x => x.Value, x => x.Key);
        }
    }
}
