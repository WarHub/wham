// WarHub licenses this file to you under the MIT license.
// See LICENSE file in the project root for more information.

namespace WarHub.Armoury.Model
{
    /// <summary>
    ///     Declares being part of catalogue.
    /// </summary>
    public interface ICatalogueItem : ICatalogueContextProvider
    {
        /// <summary>
        ///     Context being null invalidates object. Setting new context sets it recursively on any children.
        /// </summary>
        new ICatalogueContext Context { get; set; }
    }
}
