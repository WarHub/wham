namespace WarHub.ArmouryModel.Source
{
    /// <summary>
    /// This object is a Core of some Node.
    /// </summary>
    /// <typeparam name="TNode">Type of the Node this instance is a Core of.</typeparam>
    public interface ICore<out TNode> where TNode : SourceNode
    {
        /// <summary>
        /// Create an instance of the Node based on this Core.
        /// </summary>
        /// <param name="parent">Optional parent node of the created Node.</param>
        /// <returns>The newly created Node.</returns>
        TNode ToNode(SourceNode? parent = null);
    }
}
