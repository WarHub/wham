// WarHub licenses this file to you under the MIT license.
// See LICENSE file in the project root for more information.

namespace WarHub.Armoury.Model.Repo
{
    using System;

    /// <summary>
    ///     Used to report roster loading progress status.
    /// </summary>
    public class LoadRosterProgressInfo : EventArgs
    {
        public LoadRosterProgressInfo(LoadRosterState state)
        {
            State = state;
            LoadingCatalogueName = null;
        }

        public LoadRosterProgressInfo(string loadingCatalogueName, int loadedCount, int totalCount)
        {
            State = LoadRosterState.LoadingRequiredCatalogues;
            LoadingCatalogueName = loadingCatalogueName;
            LoadedCataloguesCount = loadedCount;
            CataloguesToLoadCount = totalCount;
        }

        /// <summary>
        ///     Number of catalogues required for roster. Applicable only during
        ///     <see cref="LoadRosterState.LoadingRequiredCatalogues" /> .
        /// </summary>
        public int CataloguesToLoadCount { get; private set; }

        /// <summary>
        ///     Number of catalogues loaded. Applicable only during
        ///     <see cref="LoadRosterState.LoadingRequiredCatalogues" /> .
        /// </summary>
        public int LoadedCataloguesCount { get; private set; }

        /// <summary>
        ///     Name of currently loaded catalogue. Applicable only during
        ///     <see cref="LoadRosterState.LoadingRequiredCatalogues" /> .
        /// </summary>
        public string LoadingCatalogueName { get; private set; }

        public LoadRosterState State { get; private set; }
    }
}
