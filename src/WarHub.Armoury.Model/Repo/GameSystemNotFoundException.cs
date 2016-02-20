// WarHub licenses this file to you under the MIT license.
// See LICENSE file in the project root for more information.

namespace WarHub.Armoury.Model.Repo
{
    using System;

    public class GameSystemNotFoundException : Exception
    {
        public GameSystemNotFoundException()
        {
        }

        public GameSystemNotFoundException(GameSystemInfo systemInfo)
            : base($"Game system '{systemInfo.Name}' not found" +
                   $" (v{systemInfo.Revision}, by {systemInfo.AuthorName}, id={systemInfo.RawId}).")
        {
        }

        public GameSystemNotFoundException(RosterInfo rosterInfo)
            : base($"Game system (id='{rosterInfo.GameSystemRawId}') required to edit '{rosterInfo.Name}' not found.")
        {
        }

        public GameSystemNotFoundException(CatalogueInfo catalogueInfo)
            : base($"Game system (id='{catalogueInfo.GameSystemRawId}')" +
                   $" required to edit '{catalogueInfo.Name}' not found.")
        {
        }

        public GameSystemNotFoundException(string message) : base(message)
        {
        }

        public GameSystemNotFoundException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
