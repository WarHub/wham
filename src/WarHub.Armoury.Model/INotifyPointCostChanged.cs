// WarHub licenses this file to you under the MIT license.
// See LICENSE file in the project root for more information.

namespace WarHub.Armoury.Model
{
    using System;

    public delegate void PointCostChangedEventHandler(object sender, PointCostChangedEventArgs e);

    public interface INotifyPointCostChanged
    {
        event PointCostChangedEventHandler PointCostChanged;
    }

    public class PointCostChangedEventArgs : EventArgs
    {
    }
}
