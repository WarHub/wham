namespace WarHub.Armoury.Model.BattleScribe
{
    using System;

    public class CatalogueContext : ICatalogueContext
    {
        public CatalogueContext(ICatalogue catalogue)
        {
            Catalogue = catalogue;
        }

        public ICatalogue Catalogue { get; }

        public IRegistry<IEntry> Entries { get; } = new Registry<IEntry>();

        public IRegistry<IEntryLink> EntryLinks { get; } = new Registry<IEntryLink>();

        public IRegistry<IGroupLink> GroupLinks { get; } = new Registry<IGroupLink>();

        public IRegistry<IGroup> Groups { get; } = new Registry<IGroup>();

        public IRegistry<IProfileLink> ProfileLinks { get; } = new Registry<IProfileLink>();

        public IRegistry<IProfile> Profiles { get; } = new Registry<IProfile>();

        public IRegistry<IRootEntry> RootEntries { get; } = new Registry<IRootEntry>();

        public IRegistry<IRootLink> RootLinks { get; } = new Registry<IRootLink>();

        public IRegistry<IRuleLink> RuleLinks { get; } = new Registry<IRuleLink>();

        public IRegistry<IRule> Rules { get; } = new Registry<IRule>();

        public IMultiLink GetLinked(IMultiLink unlinkedLink)
        {
            var id = unlinkedLink.TargetId;
            IRootLink rootLink;
            if (RootLinks.TryGetValue(id, out rootLink))
            {
                return new EntryMultiLink(rootLink);
            }
            IEntryLink entryLink;
            if (EntryLinks.TryGetValue(id, out entryLink))
            {
                return new EntryMultiLink(entryLink);
            }
            IGroupLink groupLink;
            if (GroupLinks.TryGetValue(id, out groupLink))
            {
                return new GroupMultiLink(groupLink);
            }
            IProfileLink profileLink;
            if (ProfileLinks.TryGetValue(id, out profileLink))
            {
                return new ProfileMultiLink(profileLink);
            }
            IRuleLink ruleLink;
            if (RuleLinks.TryGetValue(id, out ruleLink))
            {
                return new RuleMultiLink(ruleLink);
            }
            throw new ArgumentException(
                $"Link target not found. {nameof(unlinkedLink.TargetId)}='{unlinkedLink.TargetId}'",
                nameof(unlinkedLink));
        }
    }
}
