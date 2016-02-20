// WarHub licenses this file to you under the MIT license.
// See LICENSE file in the project root for more information.

namespace WarHub.Armoury.Model
{
    public interface IProfile : IIdentifiable, INameable, IBookIndexable,
        IHideable, ICloneable<IProfile>, ICatalogueItem
    {
        INodeSimple<ICharacteristic> Characteristics { get; }

        INodeSimple<IProfileModifier> Modifiers { get; }

        IIdLink<IProfileType> TypeLink { get; }
    }
}
