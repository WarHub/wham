using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;
using WarHub.ArmouryModel.Source;

namespace WarHub.ArmouryModel.Workspaces.Gitree
{
    class Gitree
    {
        static Gitree()
        {
            SeparatableKinds =
                new[]
                {
                    SourceKind.CatalogueList,
                    SourceKind.DatablobList,
                    SourceKind.DataIndexList,
                    SourceKind.ForceEntryList,
                    SourceKind.ForceList,
                    SourceKind.GamesystemList,
                    SourceKind.ProfileTypeList,
                    SourceKind.ProfileList,
                    SourceKind.RosterList,
                    SourceKind.RuleList,
                    SourceKind.SelectionEntryGroupList,
                    SourceKind.SelectionEntryList,
                    SourceKind.SelectionList
                }.ToImmutableHashSet();

            ChildListAliases =
                new Dictionary<string, string>
                {
                    [nameof(CatalogueNode.ForceEntries)] = "fe",
                    [nameof(CatalogueNode.ProfileTypes)] = "pt",
                    [nameof(CatalogueNode.Profiles)] = "p",
                    [nameof(CatalogueNode.Rules)] = "r",
                    [nameof(CatalogueNode.SharedProfiles)] = "sp",
                    [nameof(CatalogueNode.SharedRules)] = "sr",
                    [nameof(CatalogueNode.SharedSelectionEntryGroups)] = "sseg",
                    [nameof(CatalogueNode.SharedSelectionEntries)] = "sse",
                    [nameof(SelectionEntryNode.SelectionEntries)] = "se",
                    [nameof(SelectionEntryNode.SelectionEntryGroups)] = "seg",
                    [nameof(RosterNode.Forces)] = "f",
                    [nameof(ForceNode.Selections)] = "s",
                }
                .ToImmutableDictionary();
        }

        /// <summary>
        /// Gets a mapping of <see cref="ChildInfo.Name"/> to a shorter alias.
        /// </summary>
        public static ImmutableDictionary<string, string> ChildListAliases { get; }

        /// <summary>
        /// Gets a set of source kinds that will be separated from the entity into child folders.
        /// </summary>
        public static ImmutableHashSet<SourceKind> SeparatableKinds { get; }
    }
}
