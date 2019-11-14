﻿using System.Xml.Serialization;

namespace WarHub.ArmouryModel.Source
{
    [WhamNodeCore]
    [XmlType("category")]
    public sealed partial class CategoryCore : RosterElementBaseCore
    {
        [XmlAttribute("primary")]
        public bool Primary { get; }
    }
}
