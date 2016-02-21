// WarHub licenses this file to you under the MIT license.
// See LICENSE file in the project root for more information.

namespace WarHub.Armoury.Model.BattleScribeXml
{
    using GuidMapping;

    public interface ICatalogue : IIdentified, INamed, IAuthorable, IGuidControllable
    {
        uint Revision { get; set; }

        string BattleScribeVersion { get; set; }

        string Books { get; set; }
    }
}
