// WarHub licenses this file to you under the MIT license.
// See LICENSE file in the project root for more information.

namespace WarHub.Armoury.Model.BattleScribe
{
    using BattleScribeXml;

    public class RuleModifier
        : CatalogueModifier<string, RuleModifierAction, RuleField>,
            IRuleModifier
    {
        public RuleModifier(Modifier xml)
            : base(xml)
        {
            InitField();
            InitType();
        }

        public override string Value
        {
            get { return XmlBackend.Value; }
            set { Set(XmlBackend.Value, value, () => XmlBackend.Value = value); }
        }

        public IRuleModifier Clone()
        {
            return new RuleModifier(new Modifier(XmlBackend));
        }

        public override string ToString()
        {
            return Action == RuleModifierAction.Hide || Action == RuleModifierAction.Show
                ? Action.ToString()
                : $"{Action} {Field} {(Action == RuleModifierAction.Set ? "to" : "with")} {Value}";
        }
    }
}
