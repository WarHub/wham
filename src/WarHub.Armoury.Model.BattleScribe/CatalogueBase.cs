namespace WarHub.Armoury.Model.BattleScribe
{
    using BattleScribeXml;
    using ModelBases;

    public class CatalogueBase<T> : IdentifiedNamedModelBase<T>, ICatalogueBase
        where T : ICatalogue
    {
        private readonly AuthorDetails _author;

        public CatalogueBase(T xml)
            : base(xml)
        {
            _author = new AuthorDetails(xml);
        }

        public IAuthorDetails Author
        {
            get { return _author; }
        }

        public string BookSources
        {
            get { return XmlBackend.Books; }
            set { Set(XmlBackend.Books, value, () => XmlBackend.Books = value); }
        }

        public string OriginProgramVersion
        {
            get { return XmlBackend.BattleScribeVersion; }
        }

        public uint Revision
        {
            get { return XmlBackend.Revision; }
            set { Set(XmlBackend.Revision, value, () => XmlBackend.Revision = value); }
        }
    }
}
