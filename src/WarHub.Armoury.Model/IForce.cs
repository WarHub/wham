// WarHub licenses this file to you under the MIT license.
// See LICENSE file in the project root for more information.

namespace WarHub.Armoury.Model
{
    using System.Collections.Generic;

    /// <summary>
    ///     Describes part of the roster made from a single catalogue, defined by ForceType from GameSystem.
    /// </summary>
    public interface IForce : IIdentifiable, IForceNodeContainer,
        IRosterItem, IForceContextProvider
    {
        IIdLink<ICatalogue> CatalogueLink { get; }

        string CatalogueName { get; }

        uint CatalogueRevision { get; }

        /// <summary>
        ///     Immutable collection of categories, which are mutable themselves. Contents of collection
        ///     depends on ForceType defining this Force.
        /// </summary>
        IEnumerable<ICategoryMock> CategoryMocks { get; }

        IIdLink<IForceType> ForceTypeLink { get; }

        string ForceTypeName { get; }
    }
}
