// WarHub licenses this file to you under the MIT license.
// See LICENSE file in the project root for more information.

namespace WarHub.Armoury.Model.Repo
{
    using System;

    public enum RepoChange
    {
        Addition,
        Update,
        Removal
    }

    public enum RepoObject
    {
        GameSystem,
        Catalogue,
        Roster
    }

    public interface INotifyRepoChanged
    {
        event EventHandler<NotifyRepoChangedEventArgs> RepoChanged;
    }

    /// <summary>
    ///     Provides GameSystem-, Catalogue- or RosterInfo object describing the object that changed,
    ///     containing the newest version of the Info object.
    /// </summary>
    public class NotifyRepoChangedEventArgs : EventArgs
    {
        public NotifyRepoChangedEventArgs(GameSystemInfo gameSystemInfo, RepoChange changeType = RepoChange.Update)
        {
            ChangeType = changeType;
            ChangedRepoObjectType = RepoObject.GameSystem;
            GameSystemInfo = gameSystemInfo;
            SystemRawId = gameSystemInfo.RawId;
        }

        public NotifyRepoChangedEventArgs(CatalogueInfo catalogueInfo, RepoChange changeType = RepoChange.Update)
        {
            ChangeType = changeType;
            ChangedRepoObjectType = RepoObject.Catalogue;
            CatalogueInfo = catalogueInfo;
            SystemRawId = catalogueInfo.GameSystemRawId;
        }

        public NotifyRepoChangedEventArgs(RosterInfo rosterInfo, RepoChange changeType = RepoChange.Update)
        {
            ChangeType = changeType;
            ChangedRepoObjectType = RepoObject.Roster;
            RosterInfo = rosterInfo;
            SystemRawId = rosterInfo.GameSystemRawId;
        }

        public CatalogueInfo CatalogueInfo { get; }

        /// <summary>
        ///     Describes which type of Info object is carried. All other will be null.
        /// </summary>
        public RepoObject ChangedRepoObjectType { get; }

        /// <summary>
        ///     Describes whether object was added, updated or deleted.
        /// </summary>
        public RepoChange ChangeType { get; private set; }

        public GameSystemInfo GameSystemInfo { get; }

        public RosterInfo RosterInfo { get; }

        /// <summary>
        ///     Always non-null, describes to which game system this event belongs.
        /// </summary>
        public string SystemRawId { get; private set; }

        /// <summary>
        ///     Calls one of the provided callbacks depending on which info is carried in this event args.
        /// </summary>
        /// <param name="systemInfoCallback">Called if GameSystemInfo is carried.</param>
        /// <param name="catalogueInfoCallback">Called if CatalogueInfo is carried.</param>
        /// <param name="rosterInfoCallback">Called if RosterInfo is carried.</param>
        public void VisitInfo(
            Action<GameSystemInfo> systemInfoCallback,
            Action<CatalogueInfo> catalogueInfoCallback,
            Action<RosterInfo> rosterInfoCallback)
        {
            switch (ChangedRepoObjectType)
            {
                case RepoObject.GameSystem:
                    systemInfoCallback(GameSystemInfo);
                    break;

                case RepoObject.Catalogue:
                    catalogueInfoCallback(CatalogueInfo);
                    break;

                case RepoObject.Roster:
                    rosterInfoCallback(RosterInfo);
                    break;
            }
        }
    }
}
