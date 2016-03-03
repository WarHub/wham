namespace WarHub.Armoury.Model.BattleScribe
{
    public class RootEntry : Entry, IRootEntry
    {
        private readonly IdLink<ICategory> _categoryLink;
        private ICatalogueContext _context;

        public RootEntry(BattleScribeXml.Entry xml)
            : base(xml)
        {
            _categoryLink = new IdLink<ICategory>(
                XmlBackend.CategoryGuid,
                newGuid => XmlBackend.CategoryGuid = newGuid,
                () => XmlBackend.CategoryId);
        }

        public IIdLink<ICategory> CategoryLink => _categoryLink;

        public override ICatalogueContext Context
        {
            get { return _context; }
            set
            {
                var old = _context;
                if (!Set(ref _context, value))
                {
                    return;
                }
                old?.RootEntries.Deregister(this);
                if (value != null)
                {
                    value.RootEntries.Register(this);
                    value.Catalogue.SystemContext.Categories.SetTargetOf(CategoryLink);
                }
                else
                {
                    CategoryLink.Target = null;
                }
                Modifiers.ChangeContext(value);
                Profiles.ChangeContext(value);
                ProfileLinks.ChangeContext(value);
                Rules.ChangeContext(value);
                RuleLinks.ChangeContext(value);
                Groups.ChangeContext(value);
                GroupLinks.ChangeContext(value);
                Entries.ChangeContext(value);
                EntryLinks.ChangeContext(value);
            }
        }
    }
}
