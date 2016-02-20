// WarHub licenses this file to you under the MIT license.
// See LICENSE file in the project root for more information.

namespace WarHub.Armoury.Model
{
    using System.Xml.Serialization;

    public enum EntryBaseModifierAction
    {
        [XmlEnum("increment")] Increment,

        [XmlEnum("decrement")] Decrement,

        [XmlEnum("set")] Set,

        [XmlEnum("hide")] Hide,

        [XmlEnum("show")] Show
    }

    public interface IEntryBase : IIdentifiable, INameable,
        ICollectiveable, IHideable, ICatalogueItem,
        IEntriesLinkedNodeContainer, IGroupsLinkedNodeContainer
    {
        IEntryLimits Limits { get; }
    }
}
