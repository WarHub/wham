// WarHub licenses this file to you under the MIT license.
// See LICENSE file in the project root for more information.

namespace WarHub.Armoury.Model
{
    /// <summary>
    ///     Provides link to Identifiable object by having a reference. Good for late binding, because
    ///     of the TargetId property, which allows it.
    /// </summary>
    /// <typeparam name="TTarget">Type of Identifiable object, the Target of this object.</typeparam>
    public interface IIdLink<TTarget> : ILink
        where TTarget : class, IIdentifiable
    {
        /// <summary>
        ///     Target object. May return null if the link wasn't bound yet.
        /// </summary>
        TTarget Target { get; set; }
    }
}
