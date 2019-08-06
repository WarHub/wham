using System;
using System.IO;

namespace WarHub.ArmouryModel.Source
{
    public static class XmlInformation
    {
        private const string XsdVersion2_2ResourcePrefix = ThisAssembly.RootNamespace + ".DataFormat.xml.schema.v2_2.";

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

        private static Stream OpenResource(string name)
            => typeof(XmlInformation).Assembly.GetManifestResourceStream(name);

        public enum RootElement
        {
            Catalogue,
            GameSystem,
            Roster,
            DataIndex
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
