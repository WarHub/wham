// WarHub licenses this file to you under the MIT license.
// See LICENSE file in the project root for more information.

namespace WarHub.Armoury.Model.BattleScribeXml
{
    using System;
    using Repo;

    public static class IdentifiedExtensions
    {
        public static TIdentified SetNewGuid<TIdentified>(this TIdentified identified) where TIdentified : IIdentified
        {
            identified.Guid = Guid.NewGuid();
            identified.Id = identified.Guid.ToString(SampleDataInfos.GuidFormat);
            return identified;
        }
    }
}
