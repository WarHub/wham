// WarHub licenses this file to you under the MIT license.
// See LICENSE file in the project root for more information.

namespace WarHub.Armoury.Model
{
    using System.Xml.Serialization;

    public enum RuleModifierAction
    {
        [XmlEnum("set")] Set,

        [XmlEnum("append")] Append,

        [XmlEnum("hide")] Hide,

        [XmlEnum("show")] Show
    }

    public interface IRuleModifier
        : ICatalogueModifier<string, RuleModifierAction, RuleField>,
            ICloneable<IRuleModifier>
    {
    }
}
