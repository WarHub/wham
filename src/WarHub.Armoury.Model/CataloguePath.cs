// WarHub licenses this file to you under the MIT license.
// See LICENSE file in the project root for more information.

namespace WarHub.Armoury.Model
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    ///     Immutable object describing some path in catalogue.
    /// </summary>
    public class CataloguePath : IReadOnlyList<IIdentifiable>
    {
        /// <summary>
        ///     Initializes new <see cref="CataloguePath" /> with <see cref="Catalogue" /> set to provided
        ///     <paramref name="catalogue" />.
        /// </summary>
        /// <param name="catalogue">Catalogue for which this path is created.</param>
        /// <exception cref="ArgumentNullException">When parameter is null.</exception>
        public CataloguePath(ICatalogueBase catalogue)
        {
            if (catalogue == null)
                throw new ArgumentNullException(nameof(catalogue));
            Catalogue = catalogue;
            Items = new IIdentifiable[0];
        }

        private CataloguePath(CataloguePath other, IIdentifiable appendedNode)
        {
            Catalogue = other.Catalogue;
            Items = new List<IIdentifiable>(other.Items) {appendedNode};
        }

        public ICatalogueBase Catalogue { get; }

        public IReadOnlyList<IIdentifiable> Items { get; }

        public IEnumerator<IIdentifiable> GetEnumerator()
        {
            return Items.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return Items.GetEnumerator();
        }

        /// <summary>
        ///     Gets the number of elements in the collection.
        /// </summary>
        /// <returns>
        ///     The number of elements in the collection.
        /// </returns>
        public int Count => Items.Count;

        /// <summary>
        ///     Gets the element at the specified index in the read-only list.
        /// </summary>
        /// <returns>
        ///     The element at the specified index in the read-only list.
        /// </returns>
        /// <param name="index">The zero-based index of the element to get. </param>
        public IIdentifiable this[int index] => Items[index];

        /// <summary>
        ///     Creates new <see cref="CataloguePath" /> based on provided root selection.
        /// </summary>
        /// <param name="selection">Root selection to get catalogue path of.</param>
        /// <returns>New path based on provided selection.</returns>
        public static CataloguePath FromRootSelection(ISelection selection)
        {
            if (selection == null)
                throw new ArgumentNullException(nameof(selection));
            if (selection.ForceContext.Force.CategoryMocks.All(mock => !mock.Selections.Contains(selection)))
                throw new ArgumentException("Not a root selection.", nameof(selection));
            var basePath = new CataloguePath(selection.ForceContext.SourceCatalogue);
            var entryLink =
                ((IEntryMultiLink) selection.OriginEntryPath.Path.FirstOrDefault(link => link is IEntryMultiLink))?
                    .Target;
            return basePath.SelectAuto(selection.OriginEntryPath.Target, entryLink);
        }

        /// <summary>
        ///     Creates new <see cref="CataloguePath" /> based on provided root entry.
        /// </summary>
        /// <param name="entry">Root entry to be path's target.</param>
        /// <returns>Newly created path.</returns>
        public static CataloguePath FromRootEntry(IRootEntry entry)
        {
            return new CataloguePath(entry.Context.Catalogue).Select(entry);
        }

        /// <summary>
        ///     Creates new <see cref="CataloguePath" /> based on provided root link.
        /// </summary>
        /// <param name="link">Root link to be path's target.</param>
        /// <returns>Newly created path.</returns>
        public static CataloguePath FromRootLink(IRootLink link)
        {
            return new CataloguePath(link.Context.Catalogue).Select(link);
        }

        /// <summary>
        ///     Creates new path with path appended with selected node.
        /// </summary>
        /// <param name="node">Node to be appended.</param>
        /// <returns>New object with appended path.</returns>
        public CataloguePath Select(IIdentifiable node)
        {
            return new CataloguePath(this, node);
        }

        public CataloguePath Select(IEntryLink link)
        {
            return new CataloguePath(this, link).Select(link.Target);
        }

        public CataloguePath Select(IProfileLink link)
        {
            return new CataloguePath(this, link).Select(link.Target);
        }

        public CataloguePath Select(IRuleLink link)
        {
            return new CataloguePath(this, link).Select(link.Target);
        }

        public CataloguePath Select(IGroupLink link)
        {
            return new CataloguePath(this, link).Select(link.Target);
        }
    }
}
