namespace WarHub.Armoury.Model.BattleScribe
{
    using BattleScribeXml;

    public class ProfileModifier
        : CatalogueModifier<string, ProfileModifierAction, IIdentifier>,
            IProfileModifier
    {
        public ProfileModifier(Modifier xml)
            : base(xml)
        {
            InitType();
            InitField();
        }

        public override IIdentifier Field
        {
            get { return FieldEnum; }
            set { FieldEnum.Value = value.Value; }
        }

        public override string Value
        {
            get { return XmlBackend.Value; }
            set { Set(XmlBackend.Value, value, () => XmlBackend.Value = value); }
        }

        public IProfileModifier Clone()
        {
            return new ProfileModifier(new Modifier(XmlBackend));
        }

        protected new void InitField()
        {
            var targetGuid = XmlBackend.FieldCharacteristicGuid;
            FieldEnum = new Identifier(
                targetGuid,
                newGuid => XmlBackend.FieldCharacteristicGuid = newGuid,
                () => XmlBackend.Field);
        }
    }
}
