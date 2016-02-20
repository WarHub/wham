// WarHub licenses this file to you under the MIT license.
// See LICENSE file in the project root for more information.

namespace WarHub.Armoury.Model
{
    using System;

    public delegate void RosterChangedEventHandler(object sender, RosterChangedEventArgs e);

    public interface INotifyRosterChanged
    {
        event RosterChangedEventHandler RosterChanged;
    }

    public class RosterChangedEventArgs : EventArgs
    {
    }
}
