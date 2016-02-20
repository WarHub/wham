// WarHub licenses this file to you under the MIT license.
// See LICENSE file in the project root for more information.

namespace WarHub.Armoury.Model
{
    using System;

    public delegate void RegistryChangedEventHandler(object sender, RegistryChangedEventArgs e);

    public enum RegistryChange
    {
        ItemAdded,
        ItemRemoved,
        ItemPropertyChanged
    }

    public interface INotifyRegistryChanged
    {
        event RegistryChangedEventHandler RegistryChanged;
    }

    public class RegistryChangedEventArgs : EventArgs
    {
        public RegistryChangedEventArgs(
            object changedObject,
            RegistryChange changeType,
            string changedPropertyName = null)
        {
            ChangedObject = changedObject;
            ChangeType = changeType;
            ChangedPropertyName = changedPropertyName;
        }

        public object ChangedObject { get; }

        public string ChangedPropertyName { get; }

        public RegistryChange ChangeType { get; }
    }
}
