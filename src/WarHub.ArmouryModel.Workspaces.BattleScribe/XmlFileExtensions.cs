using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace WarHub.ArmouryModel.Workspaces.BattleScribe
{
    /// <summary>
    /// Defines constants for BattleScribe XML file extensions,
    /// maps them to and from <see cref="XmlDocumentKind"/>
    /// and provides sets of zipped and not-zipped extensions.
    /// </summary>
    public static class XmlFileExtensions
    {
        public const string Gamesystem = ".gst";
        public const string GamesystemZipped = ".gstz";
        public const string Catalogue = ".cat";
        public const string CatalogueZipped = ".catz";
        public const string Roster = ".ros";
        public const string RosterZipped = ".rosz";
        public const string DataIndex = ".xml";
        public const string DataIndexZipped = ".bsi";

        public static ImmutableHashSet<string> Extensions { get; }
            = ImmutableHashSet.Create(
                StringComparer.OrdinalIgnoreCase,
                Gamesystem, GamesystemZipped,
                Catalogue, CatalogueZipped,
                Roster, RosterZipped,
                DataIndex, DataIndexZipped);

        public static ImmutableHashSet<string> UnzippedExtensions { get; }
            = ImmutableHashSet.Create(
                StringComparer.OrdinalIgnoreCase,
                Gamesystem,
                Catalogue,
                Roster,
                DataIndex);

        public static ImmutableHashSet<string> ZippedExtensions { get; }
            = ImmutableHashSet.Create(
                StringComparer.OrdinalIgnoreCase,
                GamesystemZipped,
                CatalogueZipped,
                RosterZipped,
                DataIndexZipped);

        public static ImmutableDictionary<XmlDocumentKind, ImmutableArray<string>> ExtensionsByKinds { get; }
            = new Dictionary<XmlDocumentKind, ImmutableArray<string>>
            {
                [XmlDocumentKind.Gamesystem] = ImmutableArray.Create(Gamesystem, GamesystemZipped),
                [XmlDocumentKind.Catalogue] = ImmutableArray.Create(Catalogue, CatalogueZipped),
                [XmlDocumentKind.Roster] = ImmutableArray.Create(Roster, RosterZipped),
                [XmlDocumentKind.DataIndex] = ImmutableArray.Create(DataIndex, DataIndexZipped),
            }.ToImmutableDictionary();

        public static ImmutableDictionary<string, XmlDocumentKind> KindsByExtensions { get; }
            = new Dictionary<string, XmlDocumentKind>
            {
                [Gamesystem] = XmlDocumentKind.Gamesystem,
                [GamesystemZipped] = XmlDocumentKind.Gamesystem,
                [Catalogue] = XmlDocumentKind.Catalogue,
                [CatalogueZipped] = XmlDocumentKind.Catalogue,
                [Roster] = XmlDocumentKind.Roster,
                [RosterZipped] = XmlDocumentKind.Roster,
                [DataIndex] = XmlDocumentKind.DataIndex,
                [DataIndexZipped] = XmlDocumentKind.DataIndex,
            }.ToImmutableDictionary(StringComparer.OrdinalIgnoreCase);
    }
}
