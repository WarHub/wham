// WarHub licenses this file to you under the MIT license.
// See LICENSE file in the project root for more information.

namespace WarHub.Armoury.Model
{
    using System.Xml.Serialization;

    public enum GroupField
    {
        [XmlEnum("minSelections")] MinSelections,

        [XmlEnum("maxSelections")] MaxSelections,

        [XmlEnum("minInForce")] MinInForce,

        [XmlEnum("maxInForce")] MaxInForce,

        [XmlEnum("minInRoster")] MinInRoster,

        [XmlEnum("maxInRoster")] MaxInRoster,

        [XmlEnum("minPoints")] MinPoints,

        [XmlEnum("maxPoints")] MaxPoints
    }

    public interface IGroup : IEntryBase, ICloneable<IGroup>
    {
        /// <summary>
        ///     Default subentry. May be null if group has no default choice.
        /// </summary>
        IEntry DefaultChoice { get; set; }

        INodeSimple<IGroupModifier> Modifiers { get; }
    }
}
