using System.Diagnostics.CodeAnalysis;

namespace WarHub.ArmouryModel.Source
{
    /// <summary>
    /// Extend this base class and override any of it's methods
    /// to customize this visitor. <see cref="DefaultVisit(SourceNode)"/>
    /// implementation does nothing.
    /// </summary>
    public abstract partial class SourceVisitor
    {
        /// <summary>
        /// Accept this visitor in this node if it's not <c>null</c>.
        /// </summary>
        /// <param name="node">Node to visit.</param>
        public virtual void Visit(SourceNode? node)
        {
            node?.Accept(this);
        }

        /// <summary>
        /// Does nothing.
        /// </summary>
        /// <param name="node">Node to visit.</param>
        public virtual void DefaultVisit(SourceNode node)
        {
        }
    }

    /// <summary>
    /// Extend this base class and override any of it's methods
    /// to customize this visitor. <see cref="DefaultVisit(SourceNode)"/>
    /// implementation does nothing.
    /// </summary>
    public abstract partial class SourceVisitor<TResult>
    {
        /// <summary>
        /// Accept this visitor in this node if it's not <c>null</c>.
        /// </summary>
        /// <param name="node">Node to visit.</param>
        /// <returns>The result of Accept on the node,
        /// or default value of <typeparamref name="TResult"/>.</returns>
        [return: MaybeNull]
        public virtual TResult Visit(SourceNode? node)
        {
            if (node != null)
            {
                return node.Accept(this);
            }
            return default;
        }

        /// <summary>
        /// Does nothing. Returns default value of <typeparamref name="TResult"/>.
        /// </summary>
        /// <param name="node">Node to visit.</param>
        /// <returns>Default value of <typeparamref name="TResult"/>.</returns>
        [return: MaybeNull]
        public virtual TResult DefaultVisit(SourceNode node)
        {
            return default;
        }
    }
}
