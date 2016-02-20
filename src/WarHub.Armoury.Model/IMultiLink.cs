// WarHub licenses this file to you under the MIT license.
// See LICENSE file in the project root for more information.

namespace WarHub.Armoury.Model
{
    /// <summary>
    ///     Composition interface. Defines this item as visitable by <see cref="IMultiLinkVisitor" />.
    /// </summary>
    public interface IMultiLink : ILink
    {
        /// <summary>
        ///     Calls Accept method of the visitor.
        /// </summary>
        /// <param name="visitor">The visitor to call on.</param>
        void Visit(IMultiLinkVisitor visitor);
    }

    public interface IEntryMultiLink : IIdLink<IEntryLink>, IMultiLink
    {
    }

    public interface IGroupMultiLink : IIdLink<IGroupLink>, IMultiLink
    {
    }

    public interface IProfileMultiLink : IIdLink<IProfileLink>, IMultiLink
    {
    }

    public interface IRuleMultiLink : IIdLink<IRuleLink>, IMultiLink
    {
    }

    public interface IUnlinkedMultiLink : IIdLink<IIdentifiable>, IMultiLink
    {
    }
}
