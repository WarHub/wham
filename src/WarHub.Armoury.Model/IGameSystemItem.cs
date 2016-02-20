// WarHub licenses this file to you under the MIT license.
// See LICENSE file in the project root for more information.

namespace WarHub.Armoury.Model
{
    /// <summary>
    ///     Declares being part of Game System
    /// </summary>
    public interface IGameSystemItem : IGameSystemContextProvider
    {
        /// <summary>
        ///     Context being null invalidates object. Setting new context sets it recursively on any children.
        /// </summary>
        new IGameSystemContext Context { get; set; }
    }
}
