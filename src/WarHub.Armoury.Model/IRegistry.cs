// WarHub licenses this file to you under the MIT license.
// See LICENSE file in the project root for more information.

namespace WarHub.Armoury.Model
{
    using System.Collections.Generic;

    public interface IRegistry<T> : INotifyRegistryChanged, IEnumerable<T>
        where T : class, IIdentifiable
    {
        T this[IIdentifier id] { get; }
        void Register(T item);
        void Deregister(T item);
        bool IsRegistered(T item);
        bool TryGetValue(IIdentifier id, out T value);

        /// <summary>
        ///     Sets Target property of given link. In case the link's demanded target isn't registered,
        ///     the register caches that link. When the requested object registers itself, cached links
        ///     have their Target immediately set.
        /// </summary>
        /// <param name="link">The link to have it's Target set as requested by TargetId property.</param>
        void SetTargetOf(IIdLink<T> link);
    }
}
