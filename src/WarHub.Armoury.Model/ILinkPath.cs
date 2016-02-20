// WarHub licenses this file to you under the MIT license.
// See LICENSE file in the project root for more information.

namespace WarHub.Armoury.Model
{
    using System.Collections.Generic;

    /// <summary>
    ///     Provides a path of links which points to target.
    /// </summary>
    /// <typeparam name="TTarget">Type of the target.</typeparam>
    public interface ILinkPath<TTarget> : IIdLink<TTarget>
        where TTarget : class, IIdentifiable, ICatalogueItem
    {
        IReadOnlyList<IMultiLink> Path { get; }
    }
}
