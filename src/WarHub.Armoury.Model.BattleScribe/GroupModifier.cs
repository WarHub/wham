namespace WarHub.Armoury.Model.BattleScribe
{
    using BattleScribeXml;

    public class GroupModifier
        : CatalogueModifier<decimal, EntryBaseModifierAction, GroupField>, IGroupModifier
    {
        public GroupModifier(Modifier xml)
            : base(xml)
        {
            InitField();
            InitType();
        }

        public override decimal Value
        {
            get { return decimal.Parse(XmlBackend.Value); }
            set
            {
                Set(XmlBackend.Value,
                    value.ToString(),
                    () => XmlBackend.Value = value.ToString());
            }
        }

        public IGroupModifier Clone()
        {
            return new GroupModifier(new Modifier(XmlBackend));
        }
    }
}
