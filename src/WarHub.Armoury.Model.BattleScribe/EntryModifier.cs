// WarHub licenses this file to you under the MIT license.
// See LICENSE file in the project root for more information.

namespace WarHub.Armoury.Model.BattleScribe
{
    using BattleScribeXml;

    public class EntryModifier
        : CatalogueModifier<decimal, EntryBaseModifierAction, EntryField>,
            IEntryModifier
    {
        public EntryModifier(Modifier xml)
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

        public IEntryModifier Clone()
        {
            return new EntryModifier(new Modifier(XmlBackend));
        }
    }
}
