// WarHub licenses this file to you under the MIT license.
// See LICENSE file in the project root for more information.

namespace WarHub.Armoury.Model
{
    using System.ComponentModel;

    public interface ICollectiveable : INotifyPropertyChanged
    {
        bool IsCollective { get; set; }
    }
}
