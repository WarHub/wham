// WarHub licenses this file to you under the MIT license.
// See LICENSE file in the project root for more information.

namespace WarHub.Armoury.Model.BattleScribeXml.GuidMapping
{
    using System;
    using System.Collections.Generic;

    public delegate void GuidListChangedEventHandler(object sender, GuidListChangedEventArgs e);

    public interface INotifyGuidListChanged
    {
        event GuidListChangedEventHandler GuidListChanged;
    }

    public class GuidListChangedEventArgs : EventArgs
    {
        public GuidListChangedEventArgs(List<Guid> newGuidList, Action<string> idSetter)
        {
            IdSetter = idSetter;
            NewGuidList = newGuidList;
        }

        public Action<string> IdSetter { get; private set; }

        public List<Guid> NewGuidList { get; private set; }
    }
}
