using System;
using System.Collections.Generic;
using System.IO;

namespace WarHub.ArmouryModel.Source
{
    public static class XmlInformation
    {
        public static Stream OpenCatalogueXmlSchemaDefinitionStream()
        {
            return typeof(XmlInformation).Assembly.GetManifestResourceStream(ThisAssembly.RootNamespace + ".Catalogue.xsd");
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
