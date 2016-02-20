// WarHub licenses this file to you under the MIT license.
// See LICENSE file in the project root for more information.

namespace WarHub.Armoury.Model
{
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.ComponentModel;

    public interface IObservableList<T> : INotifyCollectionChanged, INotifyPropertyChanged, IList<T>
    {
    }
}
