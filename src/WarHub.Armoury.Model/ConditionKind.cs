// WarHub licenses this file to you under the MIT license.
// See LICENSE file in the project root for more information.

namespace WarHub.Armoury.Model
{
    using System.Xml.Serialization;

    public enum ConditionKind
    {
        [XmlEnum("less than")] LessThan,

        [XmlEnum("at most")] AtMost,

        [XmlEnum("equal to")] EqualTo,

        [XmlEnum("not equal to")] NotEqualTo,

        [XmlEnum("greater than")] GreaterThan,

        [XmlEnum("at least")] AtLeast,

        [XmlEnum("instance of")] InstanceOf
    }
}
