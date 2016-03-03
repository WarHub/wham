// WarHub licenses this file to you under the MIT license.
// See LICENSE file in the project root for more information.

namespace WarHub.Armoury.Model.BattleScribe
{
    using System;
    using System.Diagnostics;
    using BattleScribeXml;
    using ModelBases;

    [DebuggerDisplay("{Value}")]
    public class Identifier : ModelBase, IIdentifier
    {
        private readonly Action<Guid> _guidSetter;
        private readonly Func<string> _rawGetter;
        private Guid _guid;

        public Identifier(IIdentified xml)
        {
            _guid = xml.Guid;
            _guidSetter = newGuid => xml.Guid = newGuid;
            _rawGetter = () => xml.Id;
        }

        public Identifier(Guid guid, Action<Guid> guidSetter, Func<string> rawGetter)
        {
            _guid = guid;
            _guidSetter = guidSetter;
            _rawGetter = rawGetter;
        }

        public event IdChangedEventHandler IdChanged;

        public string RawValue => _rawGetter();

        public Guid Value
        {
            get { return _guid; }
            set
            {
                var oldValue = _guid;
                Set(_guid, value, () =>
                {
                    _guid = value;
                    _guidSetter(value);

                    // now raw is changed too, probably
                    RaiseIdChanged(oldValue, value);
                    RaisePropertyChanged(nameof(RawValue));
                });
            }
        }

        private void RaiseIdChanged(Guid oldValue, Guid newValue)
        {
            IdChanged?.Invoke(this, new IdChangedEventArgs(oldValue, newValue));
        }
    }
}
