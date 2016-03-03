namespace WarHub.Armoury.Model.BattleScribe
{
    using BattleScribeXml;
    using Nodes;

    /// <summary>
    ///     Provides an abstract base for catalogue modifiers.
    /// </summary>
    /// <typeparam name="TValue">No default implementation.</typeparam>
    /// <typeparam name="TAction">In default implementation is treated as Enum.</typeparam>
    /// <typeparam name="TField">In default implementation is treated as Enum.</typeparam>
    public abstract class CatalogueModifier<TValue, TAction, TField>
        : Modifier<TValue, TAction, TField>, ICatalogueModifier<TValue, TAction, TField>
    {
        private ICatalogueContext _context;

        protected CatalogueModifier(Modifier xml)
            : base(xml)
        {
            Conditions = new CatalogueConditionNode(() => XmlBackend.Conditions, this);
            ConditionGroups = new CatalogueConditionGroupNode(() => XmlBackend.ConditionGroups,
                this);
        }

        public ICatalogueContext Context
        {
            get { return _context; }
            set
            {
                if (!Set(ref _context, value))
                    return;
                Conditions.ChangeContext(value);
                ConditionGroups.ChangeContext(value);
            }
        }

        public INodeSimple<ICatalogueConditionGroup> ConditionGroups { get; }

        public INodeSimple<ICatalogueCondition> Conditions { get; }
    }
}
