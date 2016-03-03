// WarHub licenses this file to you under the MIT license.
// See LICENSE file in the project root for more information.

namespace WarHub.Armoury.Model.BattleScribe
{
    using BattleScribeXml;
    using ModelBases;

    /// <summary>
    ///     Provides an abstract base for modifiers.
    /// </summary>
    /// <typeparam name="TValue">No default implementation.</typeparam>
    /// <typeparam name="TAction">In default implementation is treated as Enum.</typeparam>
    /// <typeparam name="TField">In default implementation is treated as Enum.</typeparam>
    public abstract class Modifier<TValue, TAction, TField>
        : XmlBackedModelBase<Modifier>, IModifier<TValue, TAction, TField>
    {
        private readonly RepetitionInfo _repetitionInfo;
        private TAction _action;
        private TField _field;

        protected Modifier(Modifier xml)
            : base(xml)
        {
            _repetitionInfo = new RepetitionInfo(xml);
        }

        protected TAction ActionEnum
        {
            get { return _action; }
            set { _action = value; }
        }

        protected TField FieldEnum
        {
            get { return _field; }
            set { _field = value; }
        }

        public virtual TAction Action
        {
            get { return _action; }
            set { EnumSet(ref _action, value, name => XmlBackend.Type = name); }
        }

        public virtual TField Field
        {
            get { return _field; }
            set { EnumSet(ref _field, value, name => XmlBackend.Field = name); }
        }

        public IRepetitionInfo Repetition
        {
            get { return _repetitionInfo; }
        }

        public abstract TValue Value { get; set; }

        protected void InitField()
        {
            XmlBackend.Field.ParseXml(out _field);
        }

        protected void InitType()
        {
            XmlBackend.Type.ParseXml(out _action);
        }
    }
}
