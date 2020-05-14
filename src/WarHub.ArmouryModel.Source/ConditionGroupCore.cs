﻿using System.Collections.Immutable;
using System.Xml.Serialization;

namespace WarHub.ArmouryModel.Source
{
    [WhamNodeCore]
    [XmlType("conditionGroup")]
    public sealed partial class ConditionGroupCore : CommentableCore
    {
        [XmlAttribute("type")]
        public ConditionGroupKind Type { get; }

        [XmlArray("conditions")]
        public ImmutableArray<ConditionCore> Conditions { get; }

        [XmlArray("conditionGroups")]
        public ImmutableArray<ConditionGroupCore> ConditionGroups { get; }
    }
}
