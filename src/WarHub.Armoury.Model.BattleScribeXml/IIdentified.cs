// WarHub licenses this file to you under the MIT license.
// See LICENSE file in the project root for more information.

namespace WarHub.Armoury.Model.BattleScribeXml
{
    using System;
    using GuidMapping;

    public interface IIdentified : INotifyGuidChanged
    {
        Guid Guid { get; set; }

        string Id { get; set; }
    }
}
