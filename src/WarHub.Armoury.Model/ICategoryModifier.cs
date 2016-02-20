// WarHub licenses this file to you under the MIT license.
// See LICENSE file in the project root for more information.

namespace WarHub.Armoury.Model
{
    using System.Xml.Serialization;

    public enum CategoryModifierAction
    {
        [XmlEnum("increment")] Increment,
        [XmlEnum("decrement")] Decrement,
        [XmlEnum("set")] Set
    }

    public interface ICategoryModifier
        : IGameSystemModifier<decimal, CategoryModifierAction, LimitField>,
            ICloneable<ICategoryModifier>
    {
    }
}
