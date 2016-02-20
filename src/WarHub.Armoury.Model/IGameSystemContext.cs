// WarHub licenses this file to you under the MIT license.
// See LICENSE file in the project root for more information.

namespace WarHub.Armoury.Model
{
    /// <summary>
    ///     Defines context for system. Context itself is immutable. It's children are not, hovewer.
    /// </summary>
    public interface IGameSystemContext
    {
        IRegistry<ICatalogue> Catalogues { get; }

        IRegistry<ICategory> Categories { get; }

        IRegistry<IForceType> ForceTypes { get; }

        /// <summary>
        ///     System is immutable. It defines context.
        /// </summary>
        IGameSystem GameSystem { get; }

        IRegistry<IProfileType> ProfileTypes { get; }
    }
}
