namespace WarHub.ArmouryModel.Source
{
    /// <summary>
    /// This a special type od Node which is a list of other nodes
    /// of the same type.
    /// </summary>
    public interface IListNode
    {
        /// <summary>
        /// Gets the <see cref="SourceKind"/> of this instance.
        /// </summary>
        SourceKind Kind { get; }

        /// <summary>
        /// Gets the <see cref="SourceKind"/> of the child elements.
        /// </summary>
        SourceKind ElementKind { get; }

        /// <summary>
        /// Gets a list of the child nodes.
        /// </summary>
        NodeList<SourceNode> NodeList { get; }
    }
}
