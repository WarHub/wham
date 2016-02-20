// WarHub licenses this file to you under the MIT license.
// See LICENSE file in the project root for more information.

namespace WarHub.Armoury.Model
{
    public interface ICategory : IIdentifiable, INameable,
        IGameSystemItem
    {
        INodeSimple<ICategoryModifier> CategoryModifiers { get; }

        ILimits<bool, bool, bool> IsAddedToParent { get; }

        ILimits<int, decimal, int> Limits { get; }
    }
}
