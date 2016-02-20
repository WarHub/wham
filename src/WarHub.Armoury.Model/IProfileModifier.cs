// WarHub licenses this file to you under the MIT license.
// See LICENSE file in the project root for more information.

namespace WarHub.Armoury.Model
{
    using System.Xml.Serialization;

    public enum ProfileModifierAction
    {
        [XmlEnum("increment")] Increment,

        [XmlEnum("decrement")] Decrement,

        [XmlEnum("append")] Append,

        [XmlEnum("hide")] Hide,

        [XmlEnum("show")] Show,

        [XmlEnum("set")] Set
    }

    public interface IProfileModifier
        : ICatalogueModifier<string, ProfileModifierAction, IIdentifier>,
            ICloneable<IProfileModifier>
    {
    }
}
