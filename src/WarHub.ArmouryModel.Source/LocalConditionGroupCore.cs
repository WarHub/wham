using System.Xml.Serialization;

namespace WarHub.ArmouryModel.Source
{
    /// <summary>
    /// NewRecruit addition: a local condition group is a child of a modifier carrying its own
    /// query (inherited from <see cref="QueryFilteredBaseCore" />), a condition <see cref="Type" />,
    /// and a repeat count. Distinct from the baseline <see cref="ConditionGroupCore" />. Not present
    /// in original BattleScribe v2.03.
    /// </summary>
    [WhamNodeCore]
    [XmlType("localConditionGroup")]
    public sealed partial record LocalConditionGroupCore : QueryFilteredBaseCore
    {
        [XmlAttribute("type")]
        public ConditionKind Type { get; init; }

        [XmlAttribute("repeats")]
        public int RepeatCount { get; init; }
    }
}
