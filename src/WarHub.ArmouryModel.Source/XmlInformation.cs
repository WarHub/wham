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
                return string.Format(XslTransformResourceFormat, GetElementName(), dataVersion.ToFilepathString());
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

        public enum RootElement
        {
            Catalogue,
            GameSystem,
            Roster,
            DataIndex
        }

        public enum BsDataVersion
        {
            v1_15,
            v2_00,
            v2_01,
            v2_02
        }

        public static ImmutableArray<BsDataVersion> BsDataVersions { get; }
            = new[]
            {
                BsDataVersion.v1_15,
                BsDataVersion.v2_00,
                BsDataVersion.v2_01,
                BsDataVersion.v2_02
            }.ToImmutableArray();

        public static ImmutableDictionary<BsDataVersion, string> BsDataVersionDisplayStrings { get; }
            = new Dictionary<BsDataVersion, string>
            {
                [BsDataVersion.v1_15] = "1.15",
                [BsDataVersion.v2_00] = "2.00",
                [BsDataVersion.v2_01] = "2.01",
                [BsDataVersion.v2_02] = "2.02"
            }.ToImmutableDictionary();

        public static ImmutableDictionary<BsDataVersion, string> BsDataVersionFilepathStrings { get; }
            = BsDataVersionDisplayStrings
            .Select(x => (key: x.Key, value: x.Value.Replace('.', '_')))
            .ToImmutableDictionary(x => x.key, x => x.value);

        public static string ToDisplayString(this BsDataVersion dataVersion)
        {
            return BsDataVersionDisplayStrings[dataVersion];
        }

        public static string ToFilepathString(this BsDataVersion dataVersion)
        {
            return BsDataVersionFilepathStrings[dataVersion];
        }

        public static string Namespace(RootElement rootElement)
        {
            switch (rootElement)
            {
                case RootElement.Catalogue:
                    return Namespaces.CatalogueXmlns;
                case RootElement.GameSystem:
                    return Namespaces.GamesystemXmlns;
                case RootElement.Roster:
                    return Namespaces.RosterXmlns;
                case RootElement.DataIndex:
                    return Namespaces.DataIndexXmlns;
                default:
                    throw new ArgumentOutOfRangeException(nameof(rootElement));
            }
        }

        public static class Namespaces
        {
            public const string CatalogueXmlns = "http://www.battlescribe.net/schema/catalogueSchema";
            public const string RosterXmlns = "http://www.battlescribe.net/schema/rosterSchema";
            public const string GamesystemXmlns = "http://www.battlescribe.net/schema/gameSystemSchema";
            public const string DataIndexXmlns = "http://www.battlescribe.net/schema/dataIndexSchema";
        }
    }
}
