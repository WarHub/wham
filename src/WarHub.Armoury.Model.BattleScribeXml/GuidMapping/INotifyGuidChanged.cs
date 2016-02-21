// WarHub licenses this file to you under the MIT license.
// See LICENSE file in the project root for more information.

namespace WarHub.Armoury.Model.BattleScribeXml.GuidMapping
{
    using System;

    public delegate void GuidChangedEventHandler(object sender, GuidChangedEventArgs e);

    public interface INotifyGuidChanged
    {
        event GuidChangedEventHandler GuidChanged;
    }

    public class GuidChangedEventArgs : EventArgs
    {
        public GuidChangedEventArgs(Guid newGuid, Action<string> idSetter)
        {
            IdSetter = idSetter;
            NewGuid = newGuid;
        }

        public Action<string> IdSetter { get; private set; }

        public Guid NewGuid { get; private set; }
    }
}
