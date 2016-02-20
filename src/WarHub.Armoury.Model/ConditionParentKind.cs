// WarHub licenses this file to you under the MIT license.
// See LICENSE file in the project root for more information.

namespace WarHub.Armoury.Model
{
    using System.Xml.Serialization;

    public enum ConditionParentKind
    {
        [XmlEnum(ReservedIdentifiers.RosterAncestorName)] Roster,
        [XmlEnum(ReservedIdentifiers.ForceAncestorName)] ForceType,
        [XmlEnum(ReservedIdentifiers.CategoryAncestorName)] Category,
        [XmlEnum(ReservedIdentifiers.DirectAncestorName)] DirectParent,
        [XmlEnum(ReservedIdentifiers.ReferenceName)] Reference
    }
}
