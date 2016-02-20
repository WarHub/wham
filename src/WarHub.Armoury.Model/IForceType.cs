// WarHub licenses this file to you under the MIT license.
// See LICENSE file in the project root for more information.

namespace WarHub.Armoury.Model
{
    public interface IForceType : IIdentifiable, INameable,
        IGameSystemItem
    {
        INodeSimple<ICategory> Categories { get; }

        INodeSimple<IForceType> ForceTypes { get; }

        ILimits<bool, bool, bool> IsAddedToParent { get; }

        ILimits<int, decimal, int> Limits { get; }
    }
}
