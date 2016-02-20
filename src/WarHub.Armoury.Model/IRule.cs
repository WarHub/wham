// WarHub licenses this file to you under the MIT license.
// See LICENSE file in the project root for more information.

namespace WarHub.Armoury.Model
{
    using System.Xml.Serialization;

    public enum RuleField
    {
        [XmlEnum("name")] Name,

        [XmlEnum("description")] Description
    }

    public interface IRule : IIdentifiable, INameable, IBookIndexable,
        IHideable, ICloneable<IRule>, ICatalogueItem
    {
        string DescriptionText { get; set; }

        INodeSimple<IRuleModifier> Modifiers { get; }
    }
}
