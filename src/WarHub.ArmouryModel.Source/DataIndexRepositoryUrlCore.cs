﻿using System.Xml.Serialization;

namespace WarHub.ArmouryModel.Source
{
    [WhamNodeCore]
    [XmlType("repositoryUrl")]
    public sealed partial class DataIndexRepositoryUrlCore
    {
        [XmlText]
        public string? Value { get; }
    }
}
