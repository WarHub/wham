namespace WarHub.Armoury.Model.BattleScribe
{
    using ModelBases;

    public class Condition : XmlBackedModelBase<BattleScribeXml.Condition>, ICondition
    {
        private ConditionChildKind _childKind;
        private ConditionParentKind _parentKind;

        protected Condition(BattleScribeXml.Condition xml)
            : base(xml)
        {
            ChildLink = new Link(
                XmlBackend.ChildGuid,
                newGuid => XmlBackend.ChildGuid = newGuid,
                () => XmlBackend.ChildId);
            ParentLink = new Link(
                XmlBackend.ParentGuid,
                newGuid => XmlBackend.ParentGuid = newGuid,
                () => XmlBackend.ParentId);
            ChildLink.TargetId.IdChanged += (o, args) => UpdateChildKind();
            ParentLink.TargetId.IdChanged += (o, args) => UpdateParentKind();
            UpdateChildKind();
            UpdateParentKind();
        }

        public ICondition Clone()
        {
            return new Condition(new BattleScribeXml.Condition(XmlBackend));
        }

        public ConditionKind ConditionKind
        {
            get { return XmlBackend.Type; }
            set { Set(XmlBackend.Type, value, () => XmlBackend.Type = value); }
        }

        public ConditionChildKind ChildKind
        {
            get { return _childKind; }
            set
            {
                if (Set(ref _childKind, value) && value != ConditionChildKind.Reference)
                {
                    ChildLink.TargetId.Value = ReservedIdentifiers.IdDictionary[value.XmlName()];
                }
            }
        }

        public ILink ChildLink { get; }

        public decimal ChildValue
        {
            get { return XmlBackend.Value; }
            set { Set(XmlBackend.Value, value, () => XmlBackend.Value = value); }
        }

        public ConditionValueUnit ChildValueUnit
        {
            get { return XmlBackend.Field; }
            set { Set(XmlBackend.Field, value, () => XmlBackend.Field = value); }
        }

        public ConditionParentKind ParentKind
        {
            get { return _parentKind; }
            set
            {
                if (Set(ref _parentKind, value) && value != ConditionParentKind.Reference)
                {
                    ParentLink.TargetId.Value = ReservedIdentifiers.IdDictionary[value.XmlName()];
                }
            }
        }

        public ILink ParentLink { get; }

        private void UpdateChildKind()
        {
            ChildKind = ChildLink.TargetId.Value.GetChildKindFromGuid();
        }

        private void UpdateParentKind()
        {
            ParentKind = ParentLink.TargetId.Value.GetParentKindFromGuid();
        }
    }
}
