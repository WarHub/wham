using System.Xml.Serialization;

namespace WarHub.ArmouryModel.Source
{
    public enum ModifierKind
    {
        /// <summary>
        /// Modifies the target by replacing its value with Modifier's value.
        /// Usable on Number, String, Boolean and Category fields.
        /// </summary>
        [XmlEnum("set")]
        Set,

        /// <summary>
        /// Modifies the target by increasing its value by Modifier's value.
        /// Usable on Number fields, and String fields containing a number.
        /// </summary>
        [XmlEnum("increment")]
        Increment,

        /// <summary>
        /// Modifies the target by decreasing its value by Modifier's value.
        /// Usable on Number fields, and String fields containing a number.
        /// </summary>
        [XmlEnum("decrement")]
        Decrement,

        /// <summary>
        /// Modifies the target by appending its value with Modifier's value.
        /// Usable on String fields.
        /// </summary>
        [XmlEnum("append")]
        Append,

        /// <summary>
        /// Modifies the target by adding the category specified by Modifier's value.
        /// Usable on Category fields.
        /// </summary>
        [XmlEnum("add")]
        Add,

        /// <summary>
        /// Modifies the target by removing the category specified by Modifier's value.
        /// Usable on Category fields.
        /// </summary>
        [XmlEnum("remove")]
        Remove,

        /// <summary>
        /// Modifies the target by making the category specified by Modifier's value primary.
        /// Usable on Category fields.
        /// </summary>
        [XmlEnum("set-primary")]
        SetPrimary,

        /// <summary>
        /// Modifies the target by making the category specified by Modifier's value non-primary.
        /// Usable on Category fields.
        /// </summary>
        [XmlEnum("unset-primary")]
        UnsetPrimary
    }
}
