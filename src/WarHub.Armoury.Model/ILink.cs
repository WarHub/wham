// WarHub licenses this file to you under the MIT license.
// See LICENSE file in the project root for more information.

namespace WarHub.Armoury.Model
{
    using System.ComponentModel;

    /// <summary>
    ///     Contains reference to Identifier.
    /// </summary>
    public interface ILink : INotifyPropertyChanged
    {
        /// <summary>
        ///     The Identifier of referenced object. May be null if link is an empty reference.
        /// </summary>
        IIdentifier TargetId { get; }
    }
}
