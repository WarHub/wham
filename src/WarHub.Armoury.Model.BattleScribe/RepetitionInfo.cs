// WarHub licenses this file to you under the MIT license.
// See LICENSE file in the project root for more information.

namespace WarHub.Armoury.Model.BattleScribe
{
    using BattleScribeXml;
    using ModelBases;

    public class RepetitionInfo : XmlBackedModelBase<Modifier>, IRepetitionInfo
    {
        private ConditionChildKind _childKind;
        private ConditionParentKind _parentKind;

        public RepetitionInfo(Modifier xml)
            : base(xml)
        {
            ParentLink = new Link(
                XmlBackend.RepeatParentGuid,
                newGuid => XmlBackend.RepeatParentGuid = newGuid,
                () => XmlBackend.RepeatParentId);
            ChildLink = new Link(
                XmlBackend.RepeatChildGuid,
                newGuid => XmlBackend.RepeatChildGuid = newGuid,
                () => XmlBackend.RepeatChildId);
            ChildLink.TargetId.IdChanged += (o, args) => UpdateChildKind();
            ParentLink.TargetId.IdChanged += (o, args) => UpdateParentKind();
            UpdateChildKind();
            UpdateParentKind();
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
            get { return XmlBackend.RepeatValue; }
            set { Set(XmlBackend.RepeatValue, value, () => XmlBackend.RepeatValue = value); }
        }

        public ConditionValueUnit ChildValueUnit
        {
            get { return XmlBackend.RepeatField; }
            set { Set(XmlBackend.RepeatField, value, () => XmlBackend.RepeatField = value); }
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

        public bool IsActive
        {
            get { return XmlBackend.Repeating; }
            set { Set(XmlBackend.Repeating, value, () => XmlBackend.Repeating = value); }
        }

        public uint Loops
        {
            get { return XmlBackend.NumberOfRepeats; }
            set { Set(XmlBackend.NumberOfRepeats, value, () => XmlBackend.NumberOfRepeats = value); }
        }

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
