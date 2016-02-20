// WarHub licenses this file to you under the MIT license.
// See LICENSE file in the project root for more information.

namespace WarHub.Armoury.Model.Repo
{
    using System.Threading.Tasks;

    /// <summary>
    ///     Provides access to data index (all info objects).
    /// </summary>
    public interface IDataIndexService
    {
        ISystemIndex this[GameSystemInfo systemInfo] { get; }
        ISystemIndex this[CatalogueInfo catalogueInfo] { get; }
        ISystemIndex this[RosterInfo rosterInfo] { get; }

        /// <summary>
        ///     Gets last indexing task created. If no task was run yet (because ie. index was
        ///     succesfully loaded from file), this is an empty completed task.
        /// </summary>
        Task LastIndexingTask { get; }

        IObservableReadonlySet<ISystemIndex> SystemIndexes { get; }

        void OnRepoChanged(object sender, NotifyRepoChangedEventArgs e);
        Task IndexStorageAsync();
    }
}
