// WarHub licenses this file to you under the MIT license.
// See LICENSE file in the project root for more information.

namespace WarHub.Armoury.Model
{
    using System.Xml.Serialization;

    public enum EntryField
    {
        [XmlEnum("minSelections")] MinSelections,

        [XmlEnum("maxSelections")] MaxSelections,

        [XmlEnum("minInForce")] MinInForce,

        [XmlEnum("maxInForce")] MaxInForce,

        [XmlEnum("minInRoster")] MinInRoster,

        [XmlEnum("maxInRoster")] MaxInRoster,

        [XmlEnum("minPoints")] MinPoints,

        [XmlEnum("maxPoints")] MaxPoints,

        [XmlEnum("points")] PointCost
    }

    public enum EntryType
    {
        [XmlEnum("model")] Model,

        [XmlEnum("unit")] Unit,

        [XmlEnum("upgrade")] Upgrade
    }

    public interface IEntry : IEntryBase, IBookIndexable, ICloneable<IEntry>,
        IProfilesLinkedNodeContainer, IRulesLinkedNodeContainer
    {
        INodeSimple<IEntryModifier> Modifiers { get; }

        decimal PointCost { get; set; }

        EntryType Type { get; set; }
    }
}
