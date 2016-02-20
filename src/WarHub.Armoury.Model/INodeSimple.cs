// WarHub licenses this file to you under the MIT license.
// See LICENSE file in the project root for more information.

namespace WarHub.Armoury.Model
{
    using System.ComponentModel;

    public interface INodeSimple<TElement> : IObservableList<TElement>
        where TElement : INotifyPropertyChanged
    {
        TElement AddNew();
        TElement InsertNew(int index);
    }
}
