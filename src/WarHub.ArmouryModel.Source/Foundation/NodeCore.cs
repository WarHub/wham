namespace WarHub.ArmouryModel.Source
{
    /// <summary>
    /// The base class for all Cores. Defines methods to
    /// create inheritance-supporting <see cref="ToNode(SourceNode)"/> method.
    /// </summary>
    public abstract record NodeCore : ICore<SourceNode>
    {
        private int spanLength;

        /// <summary>
        /// Create a Node from this Core.
        /// </summary>
        /// <param name="parent">Optional parent of the created Node.</param>
        /// <returns>The newly created Node.</returns>
        public abstract SourceNode ToNode(SourceNode? parent = null);

        /// <summary>
        /// Returns span length this node represents.
        /// </summary>
        public int GetSpanLength()
        {
            if (spanLength > 0) return spanLength;
            return spanLength = 1 + CalculateDescendantSpanLength();
        }

        /// <summary>
        /// Returns a calculated span length of all of this node's descendants.
        /// This doesn't include span of the node itself.
        /// </summary>
        /// <returns></returns>
        protected abstract int CalculateDescendantSpanLength();
    }
}
