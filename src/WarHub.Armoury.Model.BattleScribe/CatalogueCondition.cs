namespace WarHub.Armoury.Model.BattleScribe
{
    public class CatalogueCondition : Condition, ICatalogueCondition
    {
        private ICatalogueContext _context;

        public CatalogueCondition(BattleScribeXml.Condition xml)
            : base(xml)
        {
        }

        public ICatalogueContext Context
        {
            get { return _context; }
            set { Set(ref _context, value); }
        }
    }
}
