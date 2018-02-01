// WarHub licenses this file to you under the MIT license.
// See LICENSE file in the project root for more information.

namespace WarHub.Armoury.Model.Source
{
    using System.Collections.Generic;

    public class DatablobNode
    {
        public string Id { get; }
        
        public string Name { get; }
        
        public string Book { get; }
        
        public string Page { get; }
        
        public uint Revision { get; }
        
        public string BattleScribeVersion { get; }
        
        public string AuthorName { get; }
        
        public string AuthorContact { get; }
        
        public string AuthorUrl { get; }

        public IEnumerable<int> Profiles { get; }

        public IEnumerable<int> Rules { get; }

        public IEnumerable<int> InfoLinks { get; }

        public IEnumerable<int> CostTypes { get; }

        public IEnumerable<int> ProfileTypes { get; }

        public IEnumerable<int> ForceEntries { get; }

        public IEnumerable<int> SelectionEntries { get; }

        public IEnumerable<int> EntryLinks { get; }

        public IEnumerable<int> SharedSelectionEntries { get; }

        public IEnumerable<int> SharedSelectionEntryGroups { get; }

        public IEnumerable<int> SharedRules { get; }

        public IEnumerable<int> SharedProfiles { get; }
    }
}
