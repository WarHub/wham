// WarHub licenses this file to you under the MIT license.
// See LICENSE file in the project root for more information.

namespace WarHub.Armoury.Model.BattleScribeXml.GuidMapping
{
    using System;
    using System.Collections.Generic;
    using System.Xml.Serialization;

    //public abstract class GuidControllableBase : IGuidControllable, INotifyGuidChanged, INotifyGuidListChanged
    //{
    //    private GuidController _controller;

    //    /// <summary>
    //    ///     Setting this property will subscribe controller to this object's events.
    //    /// </summary>
    //    [XmlIgnore]
    //    public GuidController Controller
    //    {
    //        get { return _controller; }
    //        private set
    //        {
    //            _controller = value;
    //            if (value == null)
    //                return;
    //            value.SubscribeGuidChanges(this);
    //            value.SubscribeGuidListChanges(this);
    //        }
    //    }

    //    /// <summary>
    //    ///     Assigns controller to Controller.
    //    /// </summary>
    //    /// <param name="controller"></param>
    //    public virtual void Process(GuidController controller)
    //    {
    //        Controller = controller;
    //    }

    //    public event GuidChangedEventHandler GuidChanged;

    //    public event GuidListChangedEventHandler GuidListChanged;

    //    protected void TrySetAndRaise(ref List<Guid> guidList, List<Guid> newGuidList, Action<string> idSetter)
    //    {
    //        if (guidList == newGuidList)
    //        {
    //            return;
    //        }
    //        guidList = newGuidList;
    //        GuidListChanged?.Invoke(this, new GuidListChangedEventArgs(newGuidList, idSetter));
    //    }

    //    protected void TrySetAndRaise(ref Guid guid, Guid newGuid, Action<string> idSetter)
    //    {
    //        if (guid == newGuid)
    //        {
    //            return;
    //        }
    //        guid = newGuid;
    //        GuidChanged?.Invoke(this, new GuidChangedEventArgs(newGuid, idSetter));
    //    }
    //}
}
