// WarHub licenses this file to you under the MIT license.
// See LICENSE file in the project root for more information.

namespace WarHub.Armoury.Model.Repo
{
    using System;

    public class RosterSavingException : Exception
    {
        public RosterSavingException()
        {
        }

        public RosterSavingException(string message, RosterInfo rosterInfo)
            : base($"Failed to save roster '{rosterInfo.Name}'." +
                   $" Reason: {(string.IsNullOrEmpty(message) ? "unknown." : message)}")
        {
        }

        public RosterSavingException(string message, Exception innerException, RosterInfo rosterInfo)
            : base($"Failed to save roster '{rosterInfo.Name}'." +
                   $" Reason: {(string.IsNullOrEmpty(message) ? "unknown." : message)}", innerException)
        {
        }

        public RosterSavingException(string message) : base(message)
        {
        }

        public RosterSavingException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
