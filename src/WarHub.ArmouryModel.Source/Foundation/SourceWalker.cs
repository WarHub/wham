namespace WarHub.ArmouryModel.Source
{
    /// <summary>
    /// Extend this abstract class to walk whole node graph
    /// by default. <see cref="DefaultVisit(SourceNode)"/>
    /// visits all of given node's children.
    /// </summary>
    public abstract class SourceWalker : SourceVisitor
    {
        /// <summary>
        /// Visits all of node's children.
        /// </summary>
        /// <param name="node">Node to visit.</param>
        public override void DefaultVisit(SourceNode node)
        {
            foreach (var child in node.Children())
            {
                Visit(child);
            }
        }
    }
}
