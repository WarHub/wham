// WarHub licenses this file to you under the MIT license.
// See LICENSE file in the project root for more information.

namespace WarHub.Armoury.Model.BattleScribeXml
{
    using System.Collections.Generic;
    using GuidMapping;

    public interface IEntryBase : IIdentified, INamed, IGuidControllable
    {
        int MinSelections { get; set; }

        int MaxSelections { get; set; }

        int MinInForce { get; set; }

        int MaxInForce { get; set; }

        int MinInRoster { get; set; }

        int MaxInRoster { get; set; }

        decimal MinPoints { get; set; }

        decimal MaxPoints { get; set; }

        bool Collective { get; set; }

        bool Hidden { get; set; }

        List<Entry> Entries { get; set; }

        List<EntryGroup> EntryGroups { get; set; }

        LinkList Links { get; set; }
    }
}
