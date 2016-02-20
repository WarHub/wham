// WarHub licenses this file to you under the MIT license.
// See LICENSE file in the project root for more information.

namespace WarHub.Armoury.Model
{
    using System;
    using System.ComponentModel;

    public delegate void IdChangedEventHandler(object sender,
        IdChangedEventArgs e);

    /// <summary>
    ///     Unique identifier for an object. The value of an identifier is mutable. Each change is
    ///     announced through the event. The aim is to provide an object uniquely identifying something.
    ///     So object for which this is identifier should have that identifier immutable and assigned
    ///     (not null).
    /// </summary>
    public interface IIdentifier : INotifyPropertyChanged
    {
        /// <summary>
        ///     Denormalized value. May or may not parse as Guid. No explicit format expected.
        /// </summary>
        string RawValue { get; }

        /// <summary>
        ///     Guid value of this identifier. May be changed, however it's suggested not to unless necessary.
        /// </summary>
        Guid Value { get; set; }

        event IdChangedEventHandler IdChanged;
    }

    public class IdChangedEventArgs : EventArgs
    {
        public IdChangedEventArgs(Guid oldValue, Guid newValue)
        {
            OldValue = oldValue;
            NewValue = newValue;
        }

        public Guid NewValue { get; private set; }

        public Guid OldValue { get; private set; }
    }
}
