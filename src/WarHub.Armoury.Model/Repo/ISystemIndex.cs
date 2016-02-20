// WarHub licenses this file to you under the MIT license.
// See LICENSE file in the project root for more information.

namespace WarHub.Armoury.Model.Repo
{
    using System.ComponentModel;

    /// <summary>
    ///     Collects info objects concerning single game system.
    /// </summary>
    public interface ISystemIndex : INotifyPropertyChanged
    {
        IObservableReadonlySet<CatalogueInfo> CatalogueInfos { get; }

        GameSystemInfo GameSystemInfo { get; }

        string GameSystemRawId { get; }

        IObservableReadonlySet<RosterInfo> RosterInfos { get; }
    }
}
