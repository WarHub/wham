﻿using System.Collections.Immutable;
using System.Xml.Serialization;

namespace WarHub.ArmouryModel.Source
{
    [WhamNodeCore]
    [XmlType("profileType")]
    public sealed partial class ProfileTypeCore : CommentableCore
    {
        [XmlAttribute("id")]
        public string? Id { get; }

        [XmlAttribute("name")]
        public string? Name { get; }

        [XmlArray("characteristicTypes")]
        public ImmutableArray<CharacteristicTypeCore> CharacteristicTypes { get; }
    }
}
