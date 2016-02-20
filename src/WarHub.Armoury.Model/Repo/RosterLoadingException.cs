// WarHub licenses this file to you under the MIT license.
// See LICENSE file in the project root for more information.

namespace WarHub.Armoury.Model.Repo
{
    using System;

    public class RosterLoadingException : Exception
    {
        public RosterLoadingException()
        {
        }

        public RosterLoadingException(string message, RosterInfo rosterInfo)
            : base($"Failed to load roster '{rosterInfo.Name}'." +
                   $" Reason: {(string.IsNullOrEmpty(message) ? "unknown." : message)}")
        {
        }

        public RosterLoadingException(string message, Exception innerException, RosterInfo rosterInfo)
            : base($"Failed to load roster '{rosterInfo.Name}'." +
                   $" Reason: {(string.IsNullOrEmpty(message) ? "unknown." : message)}", innerException)
        {
        }

        public RosterLoadingException(string message) : base(message)
        {
        }

        public RosterLoadingException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
