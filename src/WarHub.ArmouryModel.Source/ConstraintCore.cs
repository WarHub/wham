﻿using System.Xml.Serialization;

namespace WarHub.ArmouryModel.Source
{
    [WhamNodeCore]
    [XmlType("constraint")]
    public sealed partial class ConstraintCore : QueryBaseCore
    {
        [XmlAttribute("id")]
        public string? Id { get; }

        [XmlAttribute("type")]
        public ConstraintKind Type { get; }
    }
}
