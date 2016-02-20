// WarHub licenses this file to you under the MIT license.
// See LICENSE file in the project root for more information.

namespace WarHub.Armoury.Model
{
    /// <summary>
    ///     Defines context for catalogue. Context itself is immutable. It's children are not, hovewer.
    /// </summary>
    public interface ICatalogueContext
    {
        ICatalogue Catalogue { get; }

        IRegistry<IEntry> Entries { get; }

        IRegistry<IEntryLink> EntryLinks { get; }

        IRegistry<IGroupLink> GroupLinks { get; }

        IRegistry<IGroup> Groups { get; }

        IRegistry<IProfileLink> ProfileLinks { get; }

        IRegistry<IProfile> Profiles { get; }

        IRegistry<IRootEntry> RootEntries { get; }

        IRegistry<IRootLink> RootLinks { get; }

        IRegistry<IRuleLink> RuleLinks { get; }

        IRegistry<IRule> Rules { get; }

        /// <summary>
        ///     Finds the link which MultiLink describes and returnes correctly typed subtype of MultiLink.
        /// </summary>
        /// <param name="unlinkedLink">Link describing Id of link to be found.</param>
        /// <returns>Found and created hard-typed link.</returns>
        IMultiLink GetLinked(IMultiLink unlinkedLink);
    }
}
