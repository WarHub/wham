﻿using System.Collections.Immutable;
using System.Xml.Serialization;

namespace WarHub.ArmouryModel.Source
{
    [WhamNodeCore]
    [XmlType("selectionEntry")]
    public partial class SelectionEntryCore : SelectionEntryBaseCore
    {
        [XmlAttribute("categoryEntryId")]
        public string CategoryEntryId { get; }

        [XmlAttribute("type")]
        public SelectionEntryKind Type { get; }

        [XmlArray("costs")]
        public ImmutableArray<CostCore> Costs { get; }
    }
}
