// WarHub licenses this file to you under the MIT license.
// See LICENSE file in the project root for more information.

namespace WarHub.Armoury.Model
{
    using System.ComponentModel;

    /// <summary>
    ///     An object able to be identified by its UID contained in Identifier. An Identifier of given
    ///     object is never changed, however its internal Guid property may change.
    /// </summary>
    public interface IIdentifiable : INotifyPropertyChanged
    {
        /// <summary>
        ///     Immutable uid, however it's internal value may change.
        /// </summary>
        IIdentifier Id { get; }
    }
}
