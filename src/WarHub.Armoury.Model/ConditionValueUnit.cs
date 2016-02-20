// WarHub licenses this file to you under the MIT license.
// See LICENSE file in the project root for more information.

namespace WarHub.Armoury.Model
{
    using System.Xml.Serialization;

    public enum ConditionValueUnit
    {
        [XmlEnum("selections")] Selections,

        [XmlEnum("total selections")] TotalSelections,

        [XmlEnum("points")] Points,

        [XmlEnum("points limit")] PointsLimit,

        [XmlEnum("percent")] Percent
    }
}
