using System.Xml.Serialization;

namespace WarHub.ArmouryModel.Source
{
    public enum ConditionGroupKind
    {
        [XmlEnum("and")]
        And,

        [XmlEnum("or")]
        Or,

        /// <summary>
        /// Observed in real-world wh40k-11e/NewRecruit data (not present in
        /// src/dataformat/xml/schema/latest/Catalogue.xsd's ConditionGroupKind enumeration - that
        /// XSD predates this condition group kind). Requires at least a specified count of the
        /// contained conditions/groups to be true.
        /// </summary>
        [XmlEnum("count")]
        Count
    }
}
