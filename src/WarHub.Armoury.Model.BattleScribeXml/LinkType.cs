// WarHub licenses this file to you under the MIT license.
// See LICENSE file in the project root for more information.

namespace WarHub.Armoury.Model.BattleScribeXml
{
    using System.Xml.Serialization;

    public enum LinkType
    {
        [XmlEnum("entry")] Entry,

        [XmlEnum("entry group")] EntryGroup,

        [XmlEnum("profile")] Profile,

        [XmlEnum("rule")] Rule
    }
}
