using System.Collections.Immutable;
using System.Xml.Serialization;

namespace WarHub.ArmouryModel.Source
{
    [WhamNodeCore]
    [XmlType("forceEntry")]
    public sealed partial record ForceEntryCore : ContainerEntryBaseCore
    {
        [XmlArray("forceEntries")]
        public ImmutableArray<ForceEntryCore> ForceEntries { get; init; } = ImmutableArray<ForceEntryCore>.Empty;

        [XmlArray("categoryLinks")]
        public ImmutableArray<CategoryLinkCore> CategoryLinks { get; init; } = ImmutableArray<CategoryLinkCore>.Empty;
    }
}
